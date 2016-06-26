using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;

namespace FreeRDC.Services
{
    public class MasterCore
    {
        public class DeviceEntry
        {
            public string AssignedID { get; set; }
            public string Fingerprint { get; set; }
            public IPEndPoint EndPoint { get; set; }
        }

        public List<DeviceEntry> OnlineHosts = new List<DeviceEntry>();
        public List<DeviceEntry> OnlineClients = new List<DeviceEntry>();

        private MasterDB _db;
        private MasterService _master;
        private static object _hostMutex = new object();

        public MasterCore(MasterService master)
        {
            _master = master;
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

        public DeviceEntry AddDevice(IPEndPoint ep, string fingerprint)
        {
            DeviceEntry h = new DeviceEntry();
            h.EndPoint = ep;
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
