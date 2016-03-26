using System;
using System.Data;
using System.IO;

namespace FreeRDC
{
    public class MasterDB
    {

        public DataSet Db { get; set; }
        public DataTable Data { get { return Db.Tables[0]; } }
        public object _mutex = new object();
        private string _filename;

        public MasterDB(string filename, bool force)
        {
            _filename = filename;
            Db = new DataSet();

            if (!File.Exists(_filename) || force)
                Create();

            Db.ReadXml(_filename);

            if (Db.Tables.Count == 0)
                Create();
        }

        public void Create()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn() { ColumnName = "id", DataType = typeof(int), AutoIncrement = true, AutoIncrementSeed = 1, AutoIncrementStep = 1 });
            dt.Columns.Add("dtCreated", typeof(DateTime));
            dt.Columns.Add("dtLastActive", typeof(DateTime));
            dt.Columns.Add("fingerprint", typeof(string));
            dt.Columns.Add("assignedId", typeof(string));
            Db.Tables.Add(dt);
            Db.WriteXml(_filename);
        }

        public void Save()
        {
            lock (_mutex)
            {
                Db.WriteXml(_filename + ".tmp");
                if (File.Exists(_filename))
                    File.Delete(_filename);
                File.Move(_filename + ".tmp", _filename);
            }
        }

        public void Refresh()
        {
            lock(_mutex) Db.ReadXml(_filename);
        }

    }
}
