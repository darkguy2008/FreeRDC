using FreeRDC.Network;
using SharpRUDP;

namespace FreeRDC.Server.Master
{
    public class App
    {
        public CommandConnection Connection = new CommandConnection();
        public bool IsAlive { get { return Connection.IsAlive; } }

        private MasterServer _server = new MasterServer();

        public void Start(string address, int port)
        {
            _server.Connection = Connection;
            Connection.OnCommandReceived += Connection_OnCommandReceived;
            Connection.Listen(address, port);
        }

        private void Connection_OnCommandReceived(RUDPChannel channel, CommandContainer cmd)
        {
            _server.ProcessCommand(channel, cmd);
        }
    }
}
