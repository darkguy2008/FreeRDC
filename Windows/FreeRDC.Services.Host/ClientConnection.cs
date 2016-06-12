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

        private void Connection_OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch((ECommandType)command.Type)
            {
                case ECommandType.CLIENT_LOGIN:
                    var cmdLogin = HostService.Serializer.DeserializeAs<Commands.CLIENT_LOGIN>(command.Command);
                    if (cmdLogin.Password == "123")
                        Connection.SendCommand(ep, HostSvc.AssignedID, new Commands.CLIENT_LOGIN_OK(), null);
                    break;
            }
        }
    }
}
