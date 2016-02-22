using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToDB.Mapping;

namespace FreeRDC.Master.Data
{
    [Table]
    public class host
    {
        [PrimaryKey, Identity]
        public int idhost { get; set; }

        [Column, NotNull]
        public DateTime dtcreated { get; set; }

        [Column, NotNull]
        public string slot { get; set; }

        [Column, NotNull]
        public string fingerprint { get; set; }
    }
}
