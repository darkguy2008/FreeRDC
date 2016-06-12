using FreeRDC.Services;
using FreeRDC.Services.Client;
using System.Threading;

namespace FreeRDC.LiveTests
{
    class Program
    {
        static void Main(string[] args)
        {
            MasterService m = new MasterService();
            m.Start("127.0.0.1", 8000);

            HostService h = new HostService();
            h.ConnectToMaster("127.0.0.1", 8000);

            ClientService c = new ClientService();
            c.ConnectToMaster("127.0.0.1", 8000);

            Thread.Sleep(1000);
            c.ConnectToHost(h.AssignedID);

            while (true)
                Thread.Sleep(10);
        }
    }
}
