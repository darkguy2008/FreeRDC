using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Host
{
    public class RDCHostService : RDCBaseNetwork
    {
        private TcpListener server;
        public static volatile bool IsAlive;
        public object mutexClient = new object();
        private RDCHostMasterConnection master = new RDCHostMasterConnection();
        List<RDCHostClientConnection> clients = new List<RDCHostClientConnection>();

        public string MasterServerHostname
        {
            get { return master.MasterServerHostname; }
            set { master.MasterServerHostname = value; }
        }

        public void Start()
        {
            Thread.Sleep(5000);

            master.Start();

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 10000);
            server = new TcpListener(localEndPoint);
            server.Start(100);
            StartAccept();
            IsAlive = true;
        }

        public void Stop()
        {
            while (clients.Where(x => x.Thread.IsAlive).Count() > 0)
            {
                foreach (RDCHostClientConnection c in clients)
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
                RDCHostClientConnection c = new RDCHostClientConnection()
                {
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
