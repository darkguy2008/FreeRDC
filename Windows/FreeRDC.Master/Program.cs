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

        static void Main(string[] args)
        {
            Config = INIFile.Read("Master.ini");
            int port = int.Parse(Config["Master"]["Port"]);
            Console.WriteLine("Master server started at port " + port);
            srv.Listen(port);
            while (srv.IsListening)
                Thread.Sleep(1000);
            Console.WriteLine("Master server stopped");
            Console.ReadKey();
        }
    }
}
