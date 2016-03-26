using ENet;
using FreeRDC.Common.Hardware;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FreeRDC.Network.Host
{
    public class HostService : CommandClient
    {
        public delegate void VoidDelegate();
        public delegate void dMasterConnected(string hostId);
        public event VoidDelegate OnInitializing;
        public event VoidDelegate OnInitialized;
        public event VoidDelegate OnMasterConnecting;
        public event dMasterConnected OnMasterConnected;
        public event VoidDelegate OnClientConnected;
        public event VoidDelegate OnMasterConnectionError;

        public string HostID { get; set; }
        public string Password { get; set; }
        public string Fingerprint { get; set; }

        private bool _isInitialized = false;
        private RDCScreenCapture _screencap = new RDCScreenCapture();

        public void Init()
        {
            OnInitializing?.Invoke();
            if(!_isInitialized)
                Fingerprint = HWID.GenerateFingerprint();
            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        public override void Connect(string hostname, int port)
        {
            OnMasterConnecting?.Invoke();
            base.Connect(hostname, port);
        }

        public override void ConnectTimeout()
        {
            base.ConnectTimeout();
            OnMasterConnectionError?.Invoke();
        }

        public override void OnCommandReceived(Event evt, RDCCommand cmd)
        {
            base.OnCommandReceived(evt, cmd);
            switch (cmd.Command)
            {
                case RDCCommandType.MASTER_AUTH:
                    SendCommand(evt.Peer, RDCCommandChannel.Auth, RDCCommandType.MASTER_AUTH_HOST, Fingerprint);
                    break;

                case RDCCommandType.MASTER_AUTH_HOST_OK:
                    HostID = cmd.StringData;
                    OnMasterConnected?.Invoke(cmd.StringData);
                    break;

                case RDCCommandType.HOST_CONNECT:
                    string clientId = cmd.StringData;
                    string pass = Encoding.UTF8.GetString(cmd.ByteData);
                    if (pass == Password)
                    {
                        OnClientConnected?.Invoke();
                        SendCommand(evt.Peer, RDCCommandChannel.Auth, RDCCommandType.HOST_CONNECT_OK, clientId);
                    }
                    else
                        SendCommand(evt.Peer, RDCCommandChannel.Auth, RDCCommandType.HOST_CONNECT_ERROR, clientId);
                    break;

                case RDCCommandType.HOST_GETINFO:
                    SendCommand(evt.Peer, RDCCommandChannel.Command, RDCCommandType.HOST_NEWINFO, Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height);
                    break;

                case RDCCommandType.HOST_SCREEN_REFRESH:
                    MemoryStream ms = _screencap.Capture3();
                    SendCommand(evt.Peer, RDCCommandChannel.Display, RDCCommandType.HOST_SCREEN_REFRESH_OK, HostID, ms.ToArray());
                    break;

                case RDCCommandType.HOST_MOUSE_MOVE:
                    RDCRemoteMouse.Move(int.Parse(cmd.StringData.Split(',')[0]), int.Parse(cmd.StringData.Split(',')[1]));
                    break;

                case RDCCommandType.HOST_MOUSE_DOWN:
                    RDCRemoteMouse.Down(int.Parse(cmd.StringData.Split(',')[0]), int.Parse(cmd.StringData.Split(',')[1]), (MouseButtons)int.Parse(cmd.StringData.Split(',')[2]));
                    break;

                case RDCCommandType.HOST_MOUSE_UP:
                    RDCRemoteMouse.Up(int.Parse(cmd.StringData.Split(',')[0]), int.Parse(cmd.StringData.Split(',')[1]), (MouseButtons)int.Parse(cmd.StringData.Split(',')[2]));
                    break;
            }
        }

        public void Shutdown()
        {
            Disconnect();
        }
    }
}
