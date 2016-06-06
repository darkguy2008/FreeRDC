using FreeRDC.Services;
using System.Threading;

namespace FreeRDC.LiveTests
{
    class Program
    {
        struct cmd
        {
            public int value { get; set; }
            public string name { get; set; }
            public byte[] bindata { get; set; }
        }

        struct nested
        {
            public int value { get; set; }
            public string name { get; set; }
            public cmd stuff { get; set; }
        }

        static void Main(string[] args)
        {
            /*
            CommandSerializer cs = new CommandSerializer();
            cmd c = new cmd() { value = 1, name = "Test", bindata = new byte[] { 0, 1, 2, 3 } };
            byte[] data = cs.Serialize(c);
            c = cs.DeserializeAs<cmd>(data);
            Console.ReadKey();
            nested n = new nested() { value = 1, name = "Test", stuff = c };
            byte[] ndata = cs.Serialize(n);
            n = cs.DeserializeAs<nested>(ndata);
            Console.ReadKey();
            */

            Master m = new Master();
            m.Start("127.0.0.1", 8000);

            Thread.Sleep(1000);

            Host h = new Host();
            h.ConnectToMaster("127.0.0.1", 8000);

            while (true)
                Thread.Sleep(10);
        }
    }
}
