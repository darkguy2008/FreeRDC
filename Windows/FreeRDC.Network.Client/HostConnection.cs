using ENet;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System;

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
            SendCommand(client, RDCCommandChannel.Command, RDCCommandType.HOST_CONNECT, ClientID, Encoding.UTF8.GetBytes(Password));
        }

        public override void SendTimeout(Peer destination, RDCCommandChannel channel, RDCCommandType cmd, string stringData, byte[] data, PacketFlags flags)
        {
            base.SendTimeout(destination, channel, cmd, stringData, data, flags);
            OnHostCommandTimeout?.Invoke();
        }

        public void Connect(string ip, int port, string pass)
        {
            OnHostConnecting?.Invoke(HostID);
            Password = pass;
            Connect(ip, port);
        }

        public void GetHostInfo()
        {
            SendCommand(Connection, RDCCommandChannel.Command, RDCCommandType.HOST_GETINFO, null);
        }

        public void ScreenRefresh()
        {
            SendCommand(Connection, RDCCommandChannel.Display, RDCCommandType.HOST_SCREEN_REFRESH, null);
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
                    int w = int.Parse(cmd.StringData.Split('x')[0]);
                    int h = int.Parse(cmd.StringData.Split('x')[1]);
                    OnHostInfo?.Invoke(w, h);
                    break;

                case RDCCommandType.HOST_SCREEN_REFRESH_OK:
                    OnHostScreenRefresh?.Invoke(cmd.ByteData);
                    break;

            }
        }

        public void HostMouseMove(string hostID, Point mousePos)
        {
            SendCommand(Connection, RDCCommandChannel.Command, RDCCommandType.HOST_MOUSE_MOVE, mousePos.X + "," + mousePos.Y);
        }

        public void HostKeyDown(KeyEventArgs e)
        {
            SendCommand(Connection, RDCCommandChannel.Command, RDCCommandType.HOST_KEY_DOWN, e.KeyValue.ToString() + "," + (e.Shift ? "1" : "0"));
        }

        public void HostKeyUp(KeyEventArgs e)
        {
            SendCommand(Connection, RDCCommandChannel.Command, RDCCommandType.HOST_KEY_UP, e.KeyValue.ToString() + "," + (e.Shift ? "1" : "0"));
        }

        public void HostMouseDown(int x, int y, MouseButtons buttons)
        {
            SendCommand(Connection, RDCCommandChannel.Command, RDCCommandType.HOST_MOUSE_DOWN, string.Format("{0},{1},{2}", x, y, (int)buttons));
        }

        public void HostMouseUp(int x, int y, MouseButtons buttons)
        {
            SendCommand(Connection, RDCCommandChannel.Command, RDCCommandType.HOST_MOUSE_UP, string.Format("{0},{1},{2}", x, y, (int)buttons));
        }
    }
}
