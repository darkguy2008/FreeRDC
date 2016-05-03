using FreeRDC.Common.IO;
using FreeRDC.Network.Master;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FreeRDC.Master
{
    class Program
    {
        private static MasterService srv = new MasterService();
        private static Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();

        private static Thread thConfig;

        static void Main(string[] args)
        {
            thConfig = new Thread(() =>
            {
                while(true)
                {
                    Config = INIFile.Read("Master.ini");
                    srv.SetNotice(Config["Master"]["Notice"]);
                    Thread.Sleep(10000);
                }
            });
            Config = INIFile.Read("Master.ini");
            int port = int.Parse(Config["Master"]["Port"]);
            string address = Config["Master"]["Listen"];
            Console.WriteLine("Master server started at " + address + ":" + port);
            srv.Listen(address, port);
            //thConfig.Start();
            while (srv.IsListening)
                Thread.Sleep(1000);
            Console.WriteLine("Master server stopped");
            Console.ReadKey();
        }
    }
}
