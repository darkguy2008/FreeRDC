using FreeRDC.Network;
using System;
using System.Net;

namespace FreeRDC.Services
{
    public class Master
    {
        private CommandConnection _server;
        private static CommandSerializer _cs = new CommandSerializer();

        public void Listen(string address, int port)
        {
            _server = new CommandConnection();
            _server.OnCommandReceived += Server_OnCommandReceived;
            _server.Server(address, port);
        }

        private void Server_OnCommandReceived(IPEndPoint ep, CommandContainer cmd)
        {
            switch ((ECommandType)cmd.Type)
            {
                case ECommandType.AUTH:
                    Commands.AUTH cmdAuth = _cs.DeserializeAs<Commands.AUTH>(cmd.Command);
                    _server.SendCommand(ep, "MASTER", "OMGTAG1", new Commands.AUTH_OK() { AssignedTag = "OMGTAG1", EndpointAddress = ep.ToString() }, null);
                    break;
            }
        }
    }
}
