using SharpRUDP;
using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandServer
    {
        public CommandInterface Server;
        public bool IsListening { get { return Server.State == ConnectionState.LISTEN; } }

        private JavaScriptSerializer _js = new JavaScriptSerializer();

        public CommandServer()
        {
            Server = new CommandInterface();
            Server.OnClientConnect += OnClientConnect;
            Server.OnClientDisconnect += OnClientDisconnect;
            Server.OnPacketReceived += OnPacketReceived;
        }

        public void Listen(string address, int port)
        {
            Server.Listen(address, port);
        }

        public void OnPacketReceived(RUDPPacket p)
        {
            if (p.Type == RUDPPacketType.NUL)
                return;
            if (p.Type == RUDPPacketType.DAT && p.Data.Length > 0)
            {
                Console.WriteLine("SERVER <- " + Encoding.UTF8.GetString(p.Data));
                OnCommandReceived(p.Src, _js.Deserialize<RDCCommand>(Encoding.UTF8.GetString(p.Data)));
            }
        }

        public virtual void OnClientConnect(IPEndPoint ep)
        {            
        }

        public void OnClientDisconnect(IPEndPoint ep)
        {
        }

        public virtual void OnCommandReceived(IPEndPoint source, RDCCommand cmd) { }
    }
}
