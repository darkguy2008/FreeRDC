using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FreeViewer.Services.Common;

namespace FreeViewer.Services.Host
{
    public class FreeViewerHost : BaseNetwork
    {
        private TcpListener server;
        public static volatile bool IsAlive;
        public object mutexClient = new object();
        List<FreeViewerHostClient> clients = new List<FreeViewerHostClient>();

        public void Start()
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 10000);
            server = new TcpListener(localEndPoint);
            server.Start(100);
            StartAccept();
            IsAlive = true;
        }

        public void Stop()
        {
            while (clients.Where(x => x.Thread.IsAlive).Count() > 0)
            {
                foreach (FreeViewerHostClient c in clients)
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
                FreeViewerHostClient c = new FreeViewerHostClient()
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
