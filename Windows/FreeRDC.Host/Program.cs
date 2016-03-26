using FreeRDC.Network.Host;
using System;
using System.Windows.Forms;

namespace FreeRDC.Host
{
    static class Program
    {
        public static HostService Host;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
