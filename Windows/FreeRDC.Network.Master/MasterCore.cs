using ENet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FreeRDC.Network.Master
{
    public class MasterCore
    {
        private MasterDB _db;
        private object _mutex = new object();

        public class ConnectedHost : CommandClient
        {
            public string HostID { get; set; }
            public string HardwareID { get; set; }
            public Peer Connection { get; set; }
        }

        public class ConnectedClient : CommandClient
        {
            public string ClientID { get; set; }
            public string HardwareID { get; set; }
            public Peer Connection { get; set; }
        }

        public void RemoveConnection(Peer client)
        {
            connections.RemoveAll(x => x == client);
            hosts.RemoveAll(x => x.Connection == client);
            clients.RemoveAll(x => x.Connection == client);
        }

        public void SetNotice(string notice)
        {
            RDCCommand cmdNotice = new RDCCommand()
            {
                Channel = RDCCommandChannel.Auth, // TODO: Change channel
                Command = RDCCommandType.MASTER_NOTICE,
                Data = notice
            };
            Parallel.ForEach(hosts, (x) => { x.SendCommand(x.Connection, cmdNotice); });
            Parallel.ForEach(clients, (x) => { x.SendCommand(x.Connection, cmdNotice); });
        }

        public List<Peer> connections = new List<Peer>();
        public List<ConnectedHost> hosts = new List<ConnectedHost>();
        public List<ConnectedClient> clients = new List<ConnectedClient>();

        public MasterCore()
        {
            _db = new MasterDB("master.xml", false);
        }

        // http://stackoverflow.com/questions/1054076/randomly-generated-hexadecimal-number-in-c-sharp
        private Random rnd = new Random((int)DateTime.Now.Ticks);
        private string GenerateID()
        {
            byte[] buffer = new byte[4];
            rnd.NextBytes(buffer);
            return String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }

        public void AddConnection(Peer client)
        {
            connections.Add(client);
        }

        public ConnectedClient AddClient(Peer connection, string fingerprint)
        {
            ConnectedClient c = clients.Where(x => x.HardwareID == fingerprint).SingleOrDefault();
            if (c == null)
            {
                c = new ConnectedClient()
                {
                    HardwareID = fingerprint,
                    ClientID = GenerateID(),
                    Connection = connection,
                };
                clients.Add(c);
            }
            if(connections.Contains(connection))
                connections.Remove(connection);
            return c;
        }

        public Peer? FindClientByID(string clientId)
        {
            ConnectedClient c = clients.Where(x => x.ClientID == clientId).SingleOrDefault();
            if (c != null)
                return c.Connection;
            else
                return null;
        }

        public string FindClientByConnection(Peer peer)
        {
            ConnectedClient c = clients.Where(x => x.Connection == peer).SingleOrDefault();
            if (c != null)
                return c.ClientID;
            else
                return null;
        }

        public Peer? FindHostByID(string hostId)
        {
            ConnectedHost h = hosts.Where(x => x.HostID == hostId).SingleOrDefault();
            if (h != null)
                return h.Connection;
            else
                return null;
        }

        public ConnectedHost AddHost(Peer connection, string fingerprint)
        {
            ConnectedHost h = new ConnectedHost()
            {
                HardwareID = fingerprint,
                HostID = GenerateID(),
                Connection = connection,
            };
            connections.Remove(connection);

            lock (_mutex)
            {
                DataRow host = (from x in _db.Data.AsEnumerable()
                                where x.Field<string>("fingerprint") == fingerprint
                                select x).SingleOrDefault();

                if (host == null)
                {
                    _db.Data.Rows.Add(null, DateTime.Now, DateTime.Now, h.HardwareID, h.HostID);
                    _db.Save();
                }
                else
                {
                    host.SetField("dtLastActive", DateTime.Now);
                    _db.Save();
                    h.HostID = host.Field<string>("assignedId");
                    h.HardwareID = fingerprint;
                }

                hosts.Add(h);
            }

            return h;
        }

        public Peer? ConnectToHost(Peer client, string hostId)
        {
            ConnectedHost h = hosts.Where(x => x.HostID == hostId).SingleOrDefault();
            if (h == null)
                return null;
            return h.Connection;
        }
    }
}
