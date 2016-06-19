using FreeRDC.Network;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace FreeRDC.Services.Host
{
    public class ClientConnection
    {
        public string HostID { get; set; }
        public string ClientID { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public CommandConnection Connection { get; set; }
        public HostService HostSvc { get; set; }

        private bool _isLoggedIn;
        private Thread _thClientInfo;
        private Thread _thClientScreenRefresh;
        private RDCScreenCapture _screencap = new RDCScreenCapture();
        private ManualResetEventSlim _sendScreenRefresh = new ManualResetEventSlim(false);

        public ClientConnection(IPEndPoint clientEndpoint, IPEndPoint outsideEndpoint)
        {
            Address = outsideEndpoint.Address.ToString().Split(':')[0];
            Port = outsideEndpoint.Port;
        }

        public void Listen()
        {
            _isLoggedIn = false;
            Console.WriteLine("HOST LISTENING ON {0}:{1}", Address, Port);
            Connection = new CommandConnection();
            Connection.OnCommandReceived += Connection_OnCommandReceived;
            Connection.Server(Address, Port);
        }

        private void ClientInfoThread()
        {
            while(_isLoggedIn)
            {
                Connection.SendCommand(Connection.RemoteEndPoint, HostID, HostSvc.GetHostInfoCommand());
                Thread.Sleep(5000);
            }
        }

        private void ClientScreenRefreshThread()
        {
            while(_isLoggedIn)
            {
                MemoryStream ms = _screencap.Capture3();
                Connection.SendCommand(Connection.RemoteEndPoint, HostID, new Commands.HOST_SCREENREFRESH() { Buffer = ms.ToArray() }, () => { _sendScreenRefresh.Set(); });
                _sendScreenRefresh.Wait();
            }
        }

        private void Connection_OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            if (command.ID != HostID)
                return;

            if (!_isLoggedIn)
            {
                if ((ECommandType)command.Type == ECommandType.CLIENT_LOGIN)
                {
                    var cmdLogin = HostService.Serializer.DeserializeAs<Commands.CLIENT_LOGIN>(command.Command);
                    if (cmdLogin.Password == "123")
                    {
                        Connection.RemoteEndPoint = ep;
                        Connection.SendCommand(Connection.RemoteEndPoint, HostID, new Commands.CLIENT_LOGIN_OK(), null);
                        _isLoggedIn = true;
                        _thClientInfo = new Thread(new ThreadStart(ClientInfoThread));
                        _thClientInfo.Start();
                        _thClientScreenRefresh = new Thread(new ThreadStart(ClientScreenRefreshThread));
                        _thClientScreenRefresh.Start();
                    }
                }
            } else {
                switch((ECommandType)command.Type)
                {
                    case ECommandType.CLIENT_MOUSE_MOVE:
                        var cmdMouseMove = HostService.Serializer.DeserializeAs<Commands.CLIENT_MOUSE_MOVE>(command.Command);
                        RDCRemoteMouse.Move(cmdMouseMove.MouseX, cmdMouseMove.MouseY);
                        break;
                    case ECommandType.CLIENT_MOUSE_DOWN:
                        var cmdMouseDown = HostService.Serializer.DeserializeAs<Commands.CLIENT_MOUSE_DOWN>(command.Command);
                        RDCRemoteMouse.Down(cmdMouseDown.MouseX, cmdMouseDown.MouseY, (MouseButtons)cmdMouseDown.Buttons);
                        break;
                    case ECommandType.CLIENT_MOUSE_UP:
                        var cmdMouseUp = HostService.Serializer.DeserializeAs<Commands.CLIENT_MOUSE_UP>(command.Command);
                        RDCRemoteMouse.Down(cmdMouseUp.MouseX, cmdMouseUp.MouseY, (MouseButtons)cmdMouseUp.Buttons);
                        break;
                }
            }
        }
    }
}
