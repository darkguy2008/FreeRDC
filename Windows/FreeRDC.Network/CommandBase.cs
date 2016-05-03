using SharpRUDP;
using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandBase
    {
        public CommandInterface Client { get; set; }
        public CommandInterface Server { get; set; }
        public bool IsConnected { get { return Client.State == ConnectionState.OPEN; } }
        public bool IsListening { get { return Server.State == ConnectionState.LISTEN; } }

        private JavaScriptSerializer _js = new JavaScriptSerializer();
        
        public virtual void Connect(string hostname, int port)
        {
            Client = new CommandInterface();
            Client.OnConnected += OnConnected;
            Client.OnPacketReceived += OnPacketReceived;
            Client.Connect(hostname, port);
        }

        public virtual void Listen(string address, int port)
        {
            Server = new CommandInterface();
            Server.OnClientConnect += OnClientConnect;
            Server.OnClientDisconnect += OnClientDisconnect;
            Server.OnPacketReceived += OnPacketReceived;
            Server.Listen(address, port);
        }

        public virtual void Disconnect()
        {
            Client?.Disconnect();
            Server?.Disconnect();
        }

        public virtual void OnConnected(IPEndPoint ep)
        {
        }

        public virtual void OnClientConnect(IPEndPoint ep)
        {
        }

        public void OnClientDisconnect(IPEndPoint ep)
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
