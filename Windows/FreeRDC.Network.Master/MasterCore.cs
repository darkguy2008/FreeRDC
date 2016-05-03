using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FreeRDC.Network.Master
{
    public class MasterCore
    {
        private MasterDB _db;
        private MasterService masterService;
        private object _mutex = new object();
        private Random rnd = new Random((int)DateTime.Now.Ticks); // http://stackoverflow.com/questions/1054076/randomly-generated-hexadecimal-number-in-c-sharp

        public class ConnectedHost : CommandBase
        {
            public string HostID { get; set; }
            public string HardwareID { get; set; }
            public IPEndPoint Connection { get; set; }
        }

        public class ConnectedClient : CommandBase
        {
            public string ClientID { get; set; }
            public string HardwareID { get; set; }
            public IPEndPoint Connection { get; set; }
        }

        public void RemoveConnection(IPEndPoint client)
        {
            connections.RemoveAll(x => x == client);
            hosts.RemoveAll(x => x.Connection == client);
            clients.RemoveAll(x => x.Connection == client);
        }

        public void SetNotice(string notice)
        {
            RDCCommand cmdNotice = new RDCCommand()
            {
                Command = RDCCommandType.MASTER_NOTICE,
                Data = notice
            };
            Parallel.ForEach(hosts, (x) => { masterService.Server.SendCommand(x.Connection, cmdNotice); });
            Parallel.ForEach(clients, (x) => { masterService.Server.SendCommand(x.Connection, cmdNotice); });
        }

        public List<IPEndPoint> connections = new List<IPEndPoint>();
        public List<ConnectedHost> hosts = new List<ConnectedHost>();
        public List<ConnectedClient> clients = new List<ConnectedClient>();

        public MasterCore(MasterService masterService)
        {
            _db = new MasterDB("master.xml", false);
            this.masterService = masterService;
        }

        private string GenerateID()
        {
            byte[] buffer = new byte[4];
            rnd.NextBytes(buffer);
            return string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }

        public void AddConnection(IPEndPoint client)
        {
            connections.Add(client);
        }

        public ConnectedClient AddClient(IPEndPoint connection, string fingerprint)
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

        public IPEndPoint FindClientByID(string clientId)
        {
            ConnectedClient c = clients.Where(x => x.ClientID == clientId).SingleOrDefault();
            if (c != null)
                return c.Connection;
            else
                return null;
        }

        public string FindClientByConnection(IPEndPoint client)
        {
            ConnectedClient c = clients.Where(x => x.Connection == client).SingleOrDefault();
            if (c != null)
                return c.ClientID;
            else
                return null;
        }

        public IPEndPoint FindHostByID(string hostId)
        {
            ConnectedHost h = hosts.Where(x => x.HostID == hostId).SingleOrDefault();
            if (h != null)
                return h.Connection;
            else
                return null;
        }

        public ConnectedHost AddHost(IPEndPoint connection, string fingerprint)
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

        public IPEndPoint ConnectToHost(IPEndPoint client, string hostId)
        {
            ConnectedHost h = hosts.Where(x => x.HostID == hostId).SingleOrDefault();
            if (h == null)
                return null;
            return h.Connection;
        }
    }
}
