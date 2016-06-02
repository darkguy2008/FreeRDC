using FreeRDC.Network;
using System.Net;

namespace FreeRDC.Services
{
    public class Master
    {
        CommandConnection server;

        public void Listen(string address, int port)
        {
            server = new CommandConnection();
            server.OnCommandReceived += Server_OnCommandReceived;
            server.Server(address, port);
        }

        private void Server_OnCommandReceived(IPEndPoint ep, CommandSerializer serializer, CommandContainer cmd)
        {
            switch(cmd.Type)
            {
                case ECommandType.AUTH:
                    Commands.AUTH cmdAuth = serializer.DeserializeAs<Commands.AUTH>(cmd.Command);
                    if(cmdAuth.AuthType == 1) // Host
                    {
                       server.SendCommand(ep, null, null, new Commands.AUTH_OK() { AssignedTag = "OMGTAG1" }, null);
                    }
                    break;
            }
        }
    }
}
