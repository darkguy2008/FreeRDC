using ENet;
using System.Drawing;
using System.Windows.Forms;

namespace FreeRDC.Network.Client
{
    public class HostConnection : CommandClient
    {
        public Peer Connection;

        public delegate void VoidDelegate();
        public delegate void dHostConnection(string hostId);
        public delegate void dHostInfo(int screenWidth, int screenHeight);
        public delegate void dHostScreenRefresh(byte[] imageData);
        public event dHostConnection OnHostConnected;
        public event dHostConnection OnHostConnecting;
        public event dHostConnection OnHostError;
        public event dHostInfo OnHostInfo;
        public event dHostScreenRefresh OnHostScreenRefresh;
        public event VoidDelegate OnHostCommandTimeout;

        public string HostID { get; set; }
        public string ClientID { get; set; }
        public string Password { get; set; }

        public HostConnection(string clientId, string hostId)
        {
            HostID = hostId;
            ClientID = clientId;
        }

        public override void OnConnected(Peer client)
        {
            base.OnConnected(client);
            Connection = client;
            SendCommand(client, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_CONNECT,
                SourceID = ClientID,
                Data = Password
            });
        }

        public override void SendTimeout(Peer destination, RDCCommandChannel channel, RDCCommand cmd)
        {
            base.SendTimeout(destination, channel, cmd);
            OnHostCommandTimeout?.Invoke();
        }

        // TODO: public override void ConnectTimeout()

        public void Connect(string ip, int port, string pass)
        {
            OnHostConnecting?.Invoke(HostID);
            Password = pass;
            Connect(ip, port);
        }

        public void GetHostInfo()
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_GETINFO,
                SourceID = ClientID
            });
        }

        public void ScreenRefresh()
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Display,
                Command = RDCCommandType.HOST_SCREEN_REFRESH,
                SourceID = ClientID
            });
        }

        public override void OnCommandReceived(Event evt, RDCCommand cmd)
        {
            base.OnCommandReceived(evt, cmd);
            switch(cmd.Command)
            {
                case RDCCommandType.HOST_CONNECT_OK:
                    OnHostConnected?.Invoke(HostID);
                    break;

                case RDCCommandType.HOST_CONNECT_ERROR:
                    OnHostError?.Invoke(HostID);
                    break;

                case RDCCommandType.HOST_NEWINFO:
                    RDCCommandPackets.HostInfoPacket info = cmd.CastDataAs<RDCCommandPackets.HostInfoPacket>();
                    OnHostInfo?.Invoke(info.ScreenWidth, info.ScreenHeight);
                    break;

                case RDCCommandType.HOST_SCREEN_REFRESH_OK:
                    OnHostScreenRefresh?.Invoke(cmd.Buffer);
                    break;

            }
        }

        public void HostMouseMove(string hostID, Point mousePos)
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_MOUSE_MOVE,
                SourceID = ClientID,
                Data = new RDCCommandPackets.HostMouseEvent()
                {
                    MouseX = mousePos.X,
                    MouseY = mousePos.Y,
                    Buttons = MouseButtons.None
                }
            });
        }

        public void HostMouseDown(int x, int y, MouseButtons buttons)
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_MOUSE_DOWN,
                SourceID = ClientID,
                Data = new RDCCommandPackets.HostMouseEvent()
                {
                    MouseX = x,
                    MouseY = y,
                    Buttons = buttons
                }
            });
        }

        public void HostMouseUp(int x, int y, MouseButtons buttons)
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_MOUSE_UP,
                SourceID = ClientID,
                Data = new RDCCommandPackets.HostMouseEvent()
                {
                    MouseX = x,
                    MouseY = y,
                    Buttons = buttons
                }
            });
        }

        public void HostKeyDown(KeyEventArgs e)
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_KEY_DOWN,
                SourceID = ClientID,
                Data = new RDCCommandPackets.HostKeyEvent()
                {
                    Alt = e.Alt,
                    Control = e.Control,
                    Handled = e.Handled,
                    KeyCode = e.KeyCode,
                    KeyData = e.KeyData,
                    KeyValue = e.KeyValue,
                    Modifiers = e.Modifiers,
                    Shift = e.Shift,
                    SuppressKeyPress = e.SuppressKeyPress
                }
            });
        }

        public void HostKeyUp(KeyEventArgs e)
        {
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Command,
                Command = RDCCommandType.HOST_KEY_UP,
                SourceID = ClientID,
                Data = new RDCCommandPackets.HostKeyEvent()
                {
                    Alt = e.Alt,
                    Control = e.Control,
                    Handled = e.Handled,
                    KeyCode = e.KeyCode,
                    KeyData = e.KeyData,
                    KeyValue = e.KeyValue,
                    Modifiers = e.Modifiers,
                    Shift = e.Shift,
                    SuppressKeyPress = e.SuppressKeyPress
                }
            });
        }
    }
}
