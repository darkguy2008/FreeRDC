using FreeRDC.Common.IO;
using FreeRDC.Server.Master;
using System;
using System.Threading;

namespace FreeRDC.Master
{
    public class Program
    {
        private static App _srv = new App();
        private static INIFile Config = new INIFile();
        private static Thread thConfig;

        static void Main(string[] args)
        {
            thConfig = new Thread(() =>
            {
                while (true)
                {
                    Config.Read("Master.ini");
                    Thread.Sleep(10000);
                }
            });
            Config.Read("Master.ini");
            int port = int.Parse(Config.GetValue("Master", "Port"));
            string address = Config.GetValue("Master", "Listen");
            Console.WriteLine("Master server started at " + address + ":" + port);
            _srv.Start(address, port);
            while (_srv.IsAlive)
                Thread.Sleep(1000);
            Console.WriteLine("Master server stopped");
            Console.ReadKey();
        }
    }
}
