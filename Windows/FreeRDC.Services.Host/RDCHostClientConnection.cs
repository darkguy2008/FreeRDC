using System.IO;
using System.Net.Sockets;
using System.Threading;
using FreeRDC.Services.Host.Remote;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Host
{
    public class RDCHostClientConnection : RDCBaseNetwork
    {
        public Thread Thread { get; set; }
        public TcpClient Client { get; set; }
        public TcpListener Listener { get; set; }
        public NetworkStream ClientStream { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        public bool IsAlive { get; set; }
        private volatile bool isStreaming = false;
        private RDCScreenCapture screencap = new RDCScreenCapture();
        private Thread thStream;
        long oldLen = 0; // Basic method for checking if to send a new image

        private ManualResetEvent sendDone; 

        public void Start()
        {
            IsAlive = true;
            Thread = new Thread(new ThreadStart(ClientThread));
            Thread.Start();
        }

        public void ClientThread()
        {
            DataReader = new BinaryReader(ClientStream);
            DataWriter = new BinaryWriter(ClientStream);
            while (IsAlive)
                if (ClientStream.DataAvailable)
                    ProcessCommand((RDCCommandStruct)binFmt.Deserialize(ClientStream));
        }

        public void ProcessCommand(RDCCommandStruct cmdData)
        {
            switch (cmdData.Command)
            {
                case RDCCommandType.STREAM_START:
                    if (thStream != null && !thStream.IsAlive)
                        thStream = null;
                    isStreaming = true;
                    thStream = new Thread(new ThreadStart(ThreadStream));
                    thStream.Start();
                    break;

                case RDCCommandType.STREAM_STOP:
                    isStreaming = false;
                    break;

                case RDCCommandType.MOUSE_MOVE:
                    RDCRemoteMouse.Move((RDCMouseStruct)cmdData.Payload);
                    break;

                case RDCCommandType.MOUSE_DOWN:
                    RDCRemoteMouse.Down((RDCMouseStruct)cmdData.Payload);
                    break;

                case RDCCommandType.MOUSE_UP:
                    RDCRemoteMouse.Up((RDCMouseStruct)cmdData.Payload);
                    break;
            }
        }

        public void ThreadStream()
        {
            // TODO: 1st send full image then dirty updates            
            sendDone = new ManualResetEvent(false);
            while (isStreaming)
            {
                MemoryStream ms = screencap.Capture3();
                if(oldLen != ms.Length)
                    SendCommand(DataWriter, RDCCommandType.REFRESH_FULL, ms.ToArray());
                oldLen = ms.Length;
            }
        }
    }
}
