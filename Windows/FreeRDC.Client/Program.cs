using FreeRDC.Network.Client;
using System;
using System.Windows.Forms;

namespace FreeRDC.Client
{
    static class Program
    {
        public static ClientService Client = new ClientService();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
