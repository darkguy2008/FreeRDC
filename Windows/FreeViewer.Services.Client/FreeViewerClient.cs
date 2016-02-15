using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using FreeViewer.Services.Common;
using FreeViewer.Services.Common.Commands;

namespace FreeViewer.Services.Client
{
    public class FreeViewerClient : BaseNetwork
    {
        private TcpClient client;
        private Thread thClient;
        public NetworkStream ClientStream { get; set; }
        public BinaryReader DataReader { get; set; }
        public BinaryWriter DataWriter { get; set; }

        public delegate void CommandDataEventDelegate(CommandStruct data);
        public event CommandDataEventDelegate OnScreenUpdateFull;

        public void Start()
        {
            Thread.Sleep(2000);

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
            SendCommand(DataWriter, HostCmdName.HOST_STREAM_START, null);
            while (true)
                if (ClientStream.DataAvailable)
                    ProcessCommand((CommandStruct)binFmt.Deserialize(ClientStream));
        }

        public void ProcessCommand(CommandStruct cmdData)
        {
            switch (cmdData.Command.ToUpperInvariant().Trim())
            {
                case "FULL":
                    if (OnScreenUpdateFull != null)
                        OnScreenUpdateFull.Invoke(cmdData);
                    break;
                default:
                    break;
            }
        }
        
        public void MouseMove(Point p)
        {
            SendCommand(DataWriter, HostCmdName.HOST_MOUSE_MOVE, new CommandMouseStruct()
            {
                Flags = 0,
                Buttons = MouseButtons.None,
                Position = p
            });
        }

        public void MouseDown(Point mousePos, MouseButtons buttons)
        {
            SendCommand(DataWriter, HostCmdName.HOST_MOUSE_DOWN, new CommandMouseStruct()
            {
                Flags = 0,
                Buttons = buttons,
                Position = mousePos
            });
        }

        public void MouseUp(Point mousePos, MouseButtons buttons)
        {
            SendCommand(DataWriter, HostCmdName.HOST_MOUSE_UP, new CommandMouseStruct()
            {
                Flags = 0,
                Buttons = buttons,
                Position = mousePos
            });
        }
    }
}
