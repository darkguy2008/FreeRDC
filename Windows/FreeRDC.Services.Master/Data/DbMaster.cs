using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToDB.Data;
using LinqToDB;

namespace FreeRDC.Master.Data
{
    public class DbMaster : DataConnection
    {
        public DbMaster(string provider, string cn) : base(provider, cn) { }

        public void Init()
        {
            var sp = DataProvider.GetSchemaProvider();
            var dbSchema = sp.GetSchema(this);
            if (!dbSchema.Tables.Any(t => t.TableName == "host"))
                this.CreateTable<host>();
        }

        public void InsertWithId<T>(T item)
        {
            this.InsertWithIdentity<T>(item);
        }

        public ITable<host> hosts { get { return GetTable<host>(); } }
    }
}
