using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using FreeRDC.Common;
using FreeRDC.Common.Network;
using FreeRDC.Master.Data;

namespace FreeRDC.Services.Master
{
    public class MasterService
    {
        public bool IsAlive { get; set; }
        public TcpListener Server { get; set; }

        public ConnectionManager Connections { get; set; }
        public SessionManager Sessions { get; set; }

        public MasterService(string pgsqlConnectionString)
        {
            Connections = new ConnectionManager(this);
            Sessions = new SessionManager(this, pgsqlConnectionString);            
        }

        public void Start()
        {
            IsAlive = true;
            Server = new TcpListener(IPAddress.Any, 80);
            Server.Start(100);
            StartAccept();
        }

        public void Stop()
        {
            IsAlive = false;
            Server.Stop();
        }

        public void StartAccept()
        {
            if (IsAlive)
                Server.BeginAcceptTcpClient(OnAccept, Server);
        }

        public void OnAccept(IAsyncResult res)
        {
            if (!IsAlive)
                return;
            TcpListener listener = (TcpListener)res.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(res);
            StartAccept();
            Connections.NewConnection(client);
        }
    }
}
