using FreeRDC.Network;
using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace FreeRDC.Services.Client
{
    public class HostConnection
    {
        public string ID { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public CommandConnection Connection { get; set; }

        public delegate void dlgVoidEvent();
        public delegate void dlgHostInfoEvent(int width, int height);
        public delegate void dlgHostScreenRefresh(byte[] bmpData);
        public event dlgVoidEvent OnConnected;
        public event dlgHostInfoEvent OnHostInfo;
        public event dlgHostScreenRefresh OnScreenRefresh;

        public HostConnection(IPEndPoint endpoint)
        {
            Address = endpoint.Address.ToString().Split(':')[0];
            Port = endpoint.Port;
        }

        public void Connect()
        {
            Console.WriteLine("CLIENT CONNECTION TO {0}:{1}", Address, Port);
            Connection = new CommandConnection();
            Connection.OnConnected += Connection_OnConnected;
            Connection.OnCommandReceived += Connection_OnCommandReceived;
            Connection.Client(Address, Port);
        }

        private void Connection_OnConnected(IPEndPoint ep)
        {
            EndPoint = ep;
            Connection.RemoteEndPoint = ep;
            Connection.SendCommand(Connection.RemoteEndPoint, ID, new Commands.CLIENT_LOGIN() { Password = "123" });
        }

        private void Connection_OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.CLIENT_LOGIN_OK:
                    OnConnected?.Invoke();
                    break;
                case ECommandType.HOST_INFO:
                    var cmdHostInfo = ClientService.Serializer.DeserializeAs<Commands.HOST_INFO>(command.Command);
                    OnHostInfo?.Invoke(cmdHostInfo.ScreenWidth, cmdHostInfo.ScreenHeight);
                    break;
                case ECommandType.HOST_SCREENREFRESH:
                    var cmdScreenRefresh = ClientService.Serializer.DeserializeAs<Commands.HOST_SCREENREFRESH>(command.Command);
                    OnScreenRefresh?.Invoke(cmdScreenRefresh.Buffer);
                    break;
            }
        }

        public void HostKeyDown(KeyEventArgs keyEventArgs)
        {
            throw new NotImplementedException();
        }

        public void HostKeyUp(KeyEventArgs keyEventArgs)
        {
            throw new NotImplementedException();
        }

        public void HostMouseMove(Point mousePos)
        {
            Connection.SendCommand(Connection.RemoteEndPoint, ID, new Commands.CLIENT_MOUSE_MOVE()
            {
                MouseX = mousePos.X,
                MouseY = mousePos.Y
            });
        }

        public void HostMouseDown(int x, int y, MouseButtons buttons)
        {
            Connection.SendCommand(Connection.RemoteEndPoint, ID, new Commands.CLIENT_MOUSE_DOWN()
            {
                MouseX = x,
                MouseY = y,
                Buttons = (int)buttons
            });
        }

        public void HostMouseUp(int x, int y, MouseButtons buttons)
        {
            Connection.SendCommand(Connection.RemoteEndPoint, ID, new Commands.CLIENT_MOUSE_UP()
            {
                MouseX = x,
                MouseY = y,
                Buttons = (int)buttons
            });
        }
    }
}
