using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;
using System.Net.Sockets;

namespace FreeRDC.Services.Master
{
    public class ConnectionManager
    {
        private MasterService Master;
        private object mutex = new object();

        private List<GenericConnection> Connections { get; set; }
        private List<HostConnection> OnlineHosts { get; set; }
        private List<ClientConnection> OnlineClients { get; set; }

        public ConnectionManager(MasterService master)
        {
            Master = master;
            Connections = new List<GenericConnection>();
            OnlineHosts = new List<HostConnection>();
            OnlineClients = new List<ClientConnection>();
        }

        public void NewConnection(TcpClient client)
        {
            lock (mutex)
            {
                Connections.Add(new GenericConnection()
                {
                    Master = Master,
                    Connection = client
                });
                Connections.Last().Start();
            }
        }

        public void NewHostConnection(GenericConnection connection, string fingerprint)
        {
            lock (mutex)
            {
                Connections.Remove(connection);
                OnlineHosts.Add(new HostConnection()
                {
                    Master = Master,
                    Connection = connection.Connection,
                    HardwareFingerprint = fingerprint
                });
                OnlineHosts.Last().Start();
            }
        }

        public void NewClientConnection(GenericConnection connection)
        {
            lock (mutex)
            {
                Connections.Remove(connection);
                OnlineClients.Add(new ClientConnection()
                {
                    Master = Master,
                    Connection = connection.Connection
                });
                OnlineClients.Last().Start();
            }
        }

        public HostConnection GetHostBySlotID(string slotID)
        {
            return OnlineHosts.Where(x => x.SlotID == slotID).SingleOrDefault();
        }
    }
}
