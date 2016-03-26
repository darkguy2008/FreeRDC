using FreeRDC.Network.Master;
using System;
using System.Threading;

namespace FreeRDC.Master
{
    class Program
    {
        static MasterService srv = new MasterService();

        static void Main(string[] args)
        {
            Console.WriteLine("Master server started");
            srv.Listen(80);
            while (srv.IsListening)
                Thread.Sleep(1000);
            Console.WriteLine("Master server stopped");
            Console.ReadKey();
        }
    }
}
