using FreeRDC.Network;
using System;
using System.Net;

namespace FreeRDC.Services.Host
{
    public class ClientConnection
    {
        public string ID { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public CommandConnection Connection { get; set; }
        public HostService HostSvc { get; set; }

        public ClientConnection(IPEndPoint clientEndpoint, IPEndPoint outsideEndpoint)
        {
            Address = outsideEndpoint.Address.ToString().Split(':')[0];
            Port = outsideEndpoint.Port;
        }

        public void Listen()
        {
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
                Thread.Sleep(100);
                //_sendScreenRefresh.Wait();
            }
        }

        private void Connection_OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch((ECommandType)command.Type)
            {
                case ECommandType.CLIENT_LOGIN:
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
                        RDCRemoteMouse.Up(cmdMouseUp.MouseX, cmdMouseUp.MouseY, (MouseButtons)cmdMouseUp.Buttons);
                        break;
                    case ECommandType.CLIENT_KEY_DOWN:
                        var cmdKeyDown = HostService.Serializer.DeserializeAs<Commands.CLIENT_KEY_DOWN>(command.Command);
                        RDCRemoteKeyboard.Down((short)cmdKeyDown.KeyCode, cmdKeyDown.Shift);
                        break;
                    case ECommandType.CLIENT_KEY_UP:
                        var cmdKeyUp = HostService.Serializer.DeserializeAs<Commands.CLIENT_KEY_DOWN>(command.Command);
                        RDCRemoteKeyboard.Up((short)cmdKeyUp.KeyCode, cmdKeyUp.Shift);
                        break;
                }
            }
        }
    }
}
