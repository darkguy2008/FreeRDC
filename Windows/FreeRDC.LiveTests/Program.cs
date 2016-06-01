using FreeRDC.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeRDC.LiveTests
{
    class Program
    {
        struct cmd
        {
            public int value { get; set; }
            public string name { get; set; }
        }

        struct nested
        {
            public int value { get; set; }
            public string name { get; set; }
            public cmd stuff { get; set; }
        }

        static void Main(string[] args)
        {
            CommandSerializer cs = new CommandSerializer();
            cmd c = new cmd() { value = 1, name = "Test" };
            byte[] data = cs.Serialize(c);
            c = cs.DeserializeAs<cmd>(data);
            //Console.ReadKey();
            nested n = new nested() { value = 1, name = "Test", stuff = c };
            byte[] ndata = cs.Serialize(n);
            n = cs.DeserializeAs<nested>(ndata);
            Console.ReadKey();
        }
    }
}
