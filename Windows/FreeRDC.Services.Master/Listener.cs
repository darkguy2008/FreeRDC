using FreeRDC.Network;
using System.Net;

namespace FreeRDC.Services.Master
{
    public class Listener
    {
        CommandConnection server;

        public void Listen(string address, int port)
        {
            server = new CommandConnection();
            server.OnCommandReceived += Server_OnCommandReceived;
            server.Server(address, port);
        }

        private void Server_OnCommandReceived(IPEndPoint ep, object cmd)
        {
            
        }
    }
}
