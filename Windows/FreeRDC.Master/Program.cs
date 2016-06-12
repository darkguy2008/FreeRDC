using FreeRDC.Common.IO;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FreeRDC.Master
{
    public class Program
    {
        private static Services.MasterService _srv = new Services.MasterService();
        private static Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();
        private static Thread thConfig;

        static void Main(string[] args)
        {
            thConfig = new Thread(() =>
            {
                while (true)
                {
                    Config = INIFile.Read("Master.ini");
                    Thread.Sleep(10000);
                }
            });
            Config = INIFile.Read("Master.ini");
            int port = int.Parse(Config["Master"]["Port"]);
            string address = Config["Master"]["Listen"];
            Console.WriteLine("Master server started at " + address + ":" + port);
            _srv.Start(address, port);
            while (_srv.IsListening)
                Thread.Sleep(1000);
            Console.WriteLine("Master server stopped");
            Console.ReadKey();
        }
    }
}
