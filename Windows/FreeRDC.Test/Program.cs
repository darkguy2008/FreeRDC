using FreeRDC.Network.Client;
using FreeRDC.Network.Host;
using FreeRDC.Network.Master;
using System;
using System.Threading;

namespace FreeRDC.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Master server started");

            MasterService m = new MasterService();
            m.Listen("127.0.0.1", 80);

            Console.ReadKey();

            /*
            HostService h = new HostService();
            h.Init();
            h.Connect("127.0.0.1", 80);

            Thread.Sleep(1000);
            */

            ClientService c = new ClientService();
            c.Connect("127.0.0.1", 80);
            c.OnConnectedToMaster += () =>
            {
                c.ConnectHost("5A2153C3", "123");
            };

            while (m.IsListening) Thread.Sleep(1000);
            Console.WriteLine("Master server stopped");
            Console.ReadKey();

        }
    }
}
