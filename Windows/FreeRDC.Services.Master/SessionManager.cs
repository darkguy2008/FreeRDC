using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Master.Data;
using FreeRDC.Common.Network;
using System.Threading.Tasks;

namespace FreeRDC.Services.Master
{
    public class Session
    {
        public int State { get; set; }
        public string SlotID { get; set; }
        public HostConnection Host { get; set; }
        public Dictionary<int, ClientConnection> ConnectedClients { get; set; }
        private object mutex = new object();
        private int cID = 0;

        public Session()
        {
            State = 1; // TODO: State individual per connection
            ConnectedClients = new Dictionary<int, ClientConnection>();
        }

        public void AddClient(ClientConnection client, string password)
        {
            lock (mutex)
            {
                cID++;
                ConnectedClients.Add(cID, client);                
            }
            Host.SendCommand(RDCCommandType.CLIENT_CONNECT, SlotID, cID, password);
        }

        public void AcceptClient(int cID)
        {
            ConnectedClients[cID].SendCommand(RDCCommandType.CLIENT_CONNECT_OK, SlotID, cID);
        }

        public void SendToHost(RDCCommandType cmd, object[] args)
        {
            Host.SendCommand(cmd, args);
        }

        public void SendToAllClients(RDCCommandType cmd, object[] args)
        {
            Parallel.ForEach(ConnectedClients, (client) => { client.Value.SendCommand(cmd, args); });
        }
    }

    public class SessionManager
    {
        private Random rnd = new Random();
        private MasterService Master { get; set; }
        private string PostgreSQLConnectionString { get; set; }

        public Dictionary<string, Session> Sessions = new Dictionary<string, Session>();

        public SessionManager(MasterService master, string pgsqlConnectionString)
        {
            Master = master;
            PostgreSQLConnectionString = pgsqlConnectionString;
        }

        public DbMaster DbInstance()
        {
            return new DbMaster("PostgreSQL", PostgreSQLConnectionString);
        }

        // http://stackoverflow.com/questions/1054076/randomly-generated-hexadecimal-number-in-c-sharp
        private string GenerateSlotID()
        {
            byte[] buffer = new byte[4];
            rnd.NextBytes(buffer);
            return String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }

        public string RequestSlotID(string HardwareFingerprint)
        {
            string rv = null;
            using (var db = DbInstance())
            {
                var host = db.hosts.Where(x => x.fingerprint == HardwareFingerprint).FirstOrDefault();
                if (host == null)
                {
                    string id = GenerateSlotID();
                    while (db.hosts.Where(x => x.slot == id).FirstOrDefault() != null)
                        id = GenerateSlotID();
                    host = new host()
                    {
                        dtcreated = DateTime.Now,
                        fingerprint = HardwareFingerprint,
                        slot = id
                    };
                    db.InsertWithId(host);
                }
                rv = host.slot;
            }
            return rv;
        }

        public void Connect(ClientConnection client, string slotID, string password)
        {
            slotID = slotID.ToUpperInvariant().Trim();
            if (!Sessions.ContainsKey(slotID))
                Sessions[slotID] = new Session() { SlotID = slotID };

            HostConnection host = Master.Connections.GetHostBySlotID(slotID);
            if (host != null)
                Sessions[slotID].Host = host;
            else
                throw new Exception("Invalid ID");

            Sessions[slotID].AddClient(client, password);
        }

        public void ConnectOK(string slotID, int cID)
        {
            slotID = slotID.ToUpperInvariant().Trim();
            if (!Sessions.ContainsKey(slotID))
                throw new Exception("Invalid SlotID?");
            Sessions[slotID].AcceptClient(cID);
        }

        public void SendToHost(RDCCommandType cmd, string slotID, params object[] args)
        {
            slotID = slotID.ToUpperInvariant().Trim();
            if (!Sessions.ContainsKey(slotID))
                throw new Exception("Invalid SlotID?");
            Sessions[slotID].SendToHost(cmd, args);
        }

        public void SendToAllClients(RDCCommandType cmd, string slotID, params object[] args)
        {
            slotID = slotID.ToUpperInvariant().Trim();
            if (!Sessions.ContainsKey(slotID))
                throw new Exception("Invalid SlotID?");
            Sessions[slotID].SendToAllClients(cmd, args);
        }
    }
}
