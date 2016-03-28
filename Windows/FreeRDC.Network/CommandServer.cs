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
                if (data.Length == 1)
                    if (data[0] == 0x01) // KeepAlive packet
                        return;
                OnCommandReceived(evt, _serializer.Deserialize<RDCCommand>(Encoding.UTF8.GetString(data)));
            };
            server.OnClientDisconnected += (Peer client) =>
            {
                OnClientDisconnected(client);
            };
        }

        public virtual void OnClientConnected(Peer client)
        {
        }

        public virtual void OnClientDisconnected(Peer client)
        {
        }

        public void Listen(int port)
        {
            server.Listen(port);
        }
    }
}
