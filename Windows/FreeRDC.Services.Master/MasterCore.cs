using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;

namespace FreeRDC.Services
{
    public class MasterCore
    {
        public class HostEntry
        {
            public string AssignedTag { get; set; }
            public string Fingerprint { get; set; }
            public IPEndPoint EndPoint { get; set; }
        }

        public List<HostEntry> OnlineHosts = new List<HostEntry>();

        private MasterDB _db;
        private Master _master;
        private static object _hostMutex = new object();

        public MasterCore(Master master)
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

        public HostEntry AddHost(IPEndPoint ep, string fingerprint)
        {
            HostEntry h = new HostEntry();
            h.EndPoint = ep;
            h.AssignedTag = GenerateID();
            h.Fingerprint = fingerprint;

            lock (_hostMutex)
            {
                DataRow host = (from x in _db.Data.AsEnumerable()
                                where x.Field<string>("fingerprint") == fingerprint
                                select x).SingleOrDefault();

                if (host == null)
                {
                    _db.Data.Rows.Add(null, DateTime.Now, DateTime.Now, h.Fingerprint, h.AssignedTag);
                    _db.Save();
                }
                else
                {
                    host.SetField("dtLastActive", DateTime.Now);
                    _db.Save();
                    h.AssignedTag = host.Field<string>("assignedId");
                    h.Fingerprint = fingerprint;
                }

                OnlineHosts.Add(h);
            }

            return h;
        }
    }
}
