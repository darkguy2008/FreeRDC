using SharpRUDP;
using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandClient
    {
        public CommandInterface Client { get; set; }
        public bool IsConnected { get { return Client.State == ConnectionState.OPEN; } }

        private JavaScriptSerializer _js = new JavaScriptSerializer();

        public CommandClient()
        {
            Client = new CommandInterface();
            Client.OnConnected += OnConnected;
            Client.OnPacketReceived += OnPacketReceived;
                        
            // TODO:
            // _client.OnConnectionTimeout
            // _client.OnDisconnected
        }

        public virtual void Connect(string hostname, int port)
        {
            Client.Connect(hostname, port);
        }

        public virtual void Disconnect()
        {
            Client.Disconnect();
        }

        public virtual void OnConnected(IPEndPoint ep)
        {
        }

        private void OnPacketReceived(RUDPPacket p)
        {
            if (p.Type == RUDPPacketType.NUL)
                return;
            if (p.Type == RUDPPacketType.DAT && p.Data.Length > 0)
            {
                Console.WriteLine("CLIENT <- " + Encoding.UTF8.GetString(p.Data));
                OnCommandReceived(p.Src, _js.Deserialize<RDCCommand>(Encoding.UTF8.GetString(p.Data)));
            }
        }

        public virtual void OnCommandReceived(IPEndPoint source, RDCCommand cmd) { }
    }
}
