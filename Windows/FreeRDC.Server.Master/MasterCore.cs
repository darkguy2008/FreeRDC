using SharpRUDP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FreeRDC.Server.Master
{
    public class MasterCore
    {
        public class DeviceEntry
        {
            public string AssignedID { get; set; }
            public string Fingerprint { get; set; }
            public RUDPChannel Channel { get; set; }
        }

        public List<DeviceEntry> OnlineHosts = new List<DeviceEntry>();
        public List<DeviceEntry> OnlineClients = new List<DeviceEntry>();

        private MasterDB _db;
        private static object _hostMutex = new object();

        public MasterCore()
        {
            _db = new MasterDB("Master.xml", false);
        }

        // http://stackoverflow.com/questions/1054076/randomly-generated-hexadecimal-number-in-c-sharp
        private Random rnd = new Random((int)DateTime.Now.Ticks);
        private string GenerateID()
        {
            byte[] buffer = new byte[4];
            rnd.NextBytes(buffer);
            return String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        }

        public DeviceEntry AddDevice(RUDPChannel channel, string fingerprint)
        {
            DeviceEntry h = new DeviceEntry();
            h.Channel = channel;
            h.AssignedID = GenerateID();
            h.Fingerprint = fingerprint;

            lock (_hostMutex)
            {
                DataRow host = (from x in _db.Data.AsEnumerable()
                                where x.Field<string>("fingerprint") == fingerprint
                                select x).SingleOrDefault();

                if (host == null)
                {
                    _db.Data.Rows.Add(null, DateTime.Now, DateTime.Now, h.Fingerprint, h.AssignedID);
                    _db.Save();
                }
                else
                {
                    host.SetField("dtLastActive", DateTime.Now);
                    _db.Save();
                    h.AssignedID = host.Field<string>("assignedId");
                    h.Fingerprint = fingerprint;
                }

                OnlineHosts.Add(h);
            }

            return h;
        }        
    }
}
