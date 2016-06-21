using FreeRDC.Services;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace FreeRDC.Host
{
    static class Program
    {
        public static HostService Host;
        public static string AppPath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
