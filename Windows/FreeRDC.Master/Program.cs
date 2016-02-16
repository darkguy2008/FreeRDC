using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeRDC.Master
{
    class Program
    {
        static void Main(string[] args)
        {
            MasterServer server = new MasterServer();
            server.Start();

            Console.WriteLine("Server started");
            Console.ReadKey();

            server.Stop();

            Console.WriteLine("Server stopped");
            Console.ReadKey();
        }
    }
}
