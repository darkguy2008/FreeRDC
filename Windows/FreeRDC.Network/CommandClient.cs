using ENet;
using System.Text;

namespace FreeRDC.Network
{
    public class CommandClient : CommandInterface
    {
        private RawUDPClient client { get; set; }
        public bool IsConnected { get { return client.IsConnected; } }

        public CommandClient()
        {
            client = new RawUDPClient();
            client.OnConnected += (Peer connection) =>
            {
                OnConnected(connection);
            };
            client.OnDataReceived += (Event evt, byte[] data) =>
            {
                OnCommandReceived(evt, _serializer.Deserialize<RDCCommand>(Encoding.UTF8.GetString(data)));
            };
            client.OnConnectionTimeout += () =>
            {
                ConnectTimeout();
            };
        }

        public virtual void OnConnected(Peer client)
        {
        }

        public virtual void Connect(string hostname, int port)
        {
            client.Connect(hostname, port);
        }

        public virtual void Disconnect()
        {
            client.Disconnect();
        }

        public virtual void ConnectTimeout()
        {
        }
    }
}
