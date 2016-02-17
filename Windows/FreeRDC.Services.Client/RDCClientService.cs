using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Client
{
    public class RDCClientService : RDCBaseNetwork
    {
        private TcpClient client;
        private Thread thClient;
        public NetworkStream ClientStream { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        public delegate void CommandDataEventDelegate(RDCCommandStruct data);
        public event CommandDataEventDelegate OnScreenUpdateFull;

        public void Start()
        {
            IPHostEntry ipHostInfo = Dns.Resolve("XPTEST");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 10000);

            client = new TcpClient();
            client.Connect(ipAddress.ToString(), 10000);

            thClient = new Thread(new ThreadStart(ThreadClient));
            thClient.Start();
        }

        public void ThreadClient()
        {
            ClientStream = client.GetStream();
            DataReader = new BinaryReader(ClientStream);
            DataWriter = new BinaryWriter(ClientStream);
            SendCommand(DataWriter, RDCCommandType.STREAM_START, null);
            while (true)
                if (ClientStream.DataAvailable)
                    ProcessCommand((RDCCommandStruct)binFmt.Deserialize(ClientStream));
        }

        public void ProcessCommand(RDCCommandStruct cmdData)
        {
            switch (cmdData.Command)
            {
                case RDCCommandType.REFRESH_FULL:
                    if (OnScreenUpdateFull != null)
                        OnScreenUpdateFull.Invoke(cmdData);
                    break;
                default:
                    break;
            }
        }
        
        public void MouseMove(Point p)
        {
            SendCommand(DataWriter, RDCCommandType.MOUSE_MOVE, new RDCMouseStruct()
            {
                Flags = 0,
                Buttons = MouseButtons.None,
                Position = p
            });
        }

        public void MouseDown(Point mousePos, MouseButtons buttons)
        {
            SendCommand(DataWriter, RDCCommandType.MOUSE_DOWN, new RDCMouseStruct()
            {
                Flags = 0,
                Buttons = buttons,
                Position = mousePos
            });
        }

        public void MouseUp(Point mousePos, MouseButtons buttons)
        {
            SendCommand(DataWriter, RDCCommandType.MOUSE_UP, new RDCMouseStruct()
            {
                Flags = 0,
                Buttons = buttons,
                Position = mousePos
            });
        }
    }
}
