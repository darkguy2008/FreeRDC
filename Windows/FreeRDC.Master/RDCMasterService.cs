using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using FreeRDC.Common.Network;
using System.Net;
using System.Threading;

namespace FreeRDC.Master
{
    public class RDCMasterService : RDCBaseNetwork
    {
        private TcpListener server;
        public static volatile bool IsAlive;
        public object mutexClient = new object();
        List<RDCMasterClientConnection> clients = new List<RDCMasterClientConnection>();

        public void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 80);
            server = new TcpListener(localEndPoint);
            server.Start(100);
            StartAccept();
            IsAlive = true;
        }

        public void Stop()
        {
            while (clients.Where(x => x.Thread.IsAlive).Count() > 0)
            {
                foreach (RDCMasterClientConnection c in clients)
                    c.IsAlive = false;
                Thread.Sleep(10);
            }
        }

        public void StartAccept()
        {
            server.BeginAcceptTcpClient(OnAccept, server);
        }

        public void OnAccept(IAsyncResult res)
        {
            TcpListener listener = (TcpListener)res.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(res);
            StartAccept();
            lock (mutexClient)
            {
                RDCMasterClientConnection c = new RDCMasterClientConnection()
                {
                    Parent = this,
                    Client = client,
                    Listener = listener,
                    ClientStream = client.GetStream(),
                };
                clients.Add(c);
                clients.Last().Start();
            }
        }
    }
}
