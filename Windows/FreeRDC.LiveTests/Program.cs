using FreeRDC.Server.Master;
using FreeRDC.Services.Master;
using System.Threading;

namespace FreeRDC.LiveTests
{
    class Program
    {
        static void Main(string[] args)
        {
            App m = new App();
            m.Start("127.0.0.1", 8000);

            Thread.Sleep(1000);
            MasterService ms = new MasterService()
            {
                Address = "127.0.0.1",
                Port = 8000
            };
            ms.Start();

            while (true)
                Thread.Sleep(10);
        }
    }
}
