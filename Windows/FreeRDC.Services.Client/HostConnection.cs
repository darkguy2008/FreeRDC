using FreeRDC.Network;
using System;
using System.Net;

namespace FreeRDC.Services.Client
{
    public class HostConnection
    {
        public string ID { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public CommandConnection Connection { get; set; }

        public HostConnection(IPEndPoint endpoint)
        {
            Address = endpoint.Address.ToString().Split(':')[0];
            Port = endpoint.Port;
        }

        public void Connect()
        {
            Console.WriteLine("CLIENT CONNECTION TO {0}:{1}", Address, Port);
            Connection = new CommandConnection();
            Connection.OnConnected += Connection_OnConnected;
            Connection.Client(Address, Port);
        }

        private void Connection_OnConnected(IPEndPoint ep)
        {
            EndPoint = ep;
            Connection.SendCommand(EndPoint, ID, new Commands.CLIENT_LOGIN() { Password = "123" });
        }
    }
}
