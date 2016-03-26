using ENet;
using System.Text;

namespace FreeRDC.Network
{
    public class CommandServer : CommandInterface
    {
        private RawUDPServer server { get; set; }
        public bool IsListening { get { return server.IsListening; } }

        public CommandServer()
        {
            server = new RawUDPServer();
            server.OnClientConnected += (Peer client) =>
            {
                OnClientConnected(client);
            };
            server.OnDataReceived += (Event evt, byte[] data) =>
            {
                OnCommandReceived(evt, _serializer.Deserialize<RDCCommand>(Encoding.UTF8.GetString(data)));
            };
        }

        public virtual void OnClientConnected(Peer client)
        {
        }

        public void Listen(int port)
        {
            server.Listen(port);
        }
    }
}
