using System.IO;
using System.Net.Sockets;
using System.Threading;
using FreeRDC.Services.Common;
using FreeRDC.Services.Common.Commands;
using FreeRDC.Services.Host.Remote;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Host
{
    public class FreeRDCHostClient : BaseNetwork
    {
        public Thread Thread { get; set; }
        public TcpClient Client { get; set; }
        public TcpListener Listener { get; set; }
        public NetworkStream ClientStream { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        public bool IsAlive { get; set; }
        private volatile bool isStreaming = false;
        private ScreenCapture screencap = new ScreenCapture();
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
                    ProcessCommand((CommandStruct)binFmt.Deserialize(ClientStream));
        }

        public void ProcessCommand(CommandStruct cmdData)
        {
            switch (cmdData.Command.ToUpperInvariant().Trim())
            {
                case HostCmdName.HOST_STREAM_START:
                    if (thStream != null && !thStream.IsAlive)
                        thStream = null;
                    isStreaming = true;
                    thStream = new Thread(new ThreadStart(ThreadStream));
                    thStream.Start();
                    break;

                case HostCmdName.HOST_STREAM_STOP:
                    isStreaming = false;
                    break;

                case HostCmdName.HOST_MOUSE_MOVE:
                    RemoteMouse.Move((CommandMouseStruct)cmdData.Payload);
                    break;

                case HostCmdName.HOST_MOUSE_DOWN:
                    RemoteMouse.Down((CommandMouseStruct)cmdData.Payload);
                    break;

                case HostCmdName.HOST_MOUSE_UP:
                    RemoteMouse.Up((CommandMouseStruct)cmdData.Payload);
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
                    SendCommand(DataWriter, "FULL", ms.ToArray());
                oldLen = ms.Length;
            }
        }
    }
}
