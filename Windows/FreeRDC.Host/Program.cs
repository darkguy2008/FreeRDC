using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace FreeRDC.Host
{
    static class Program
    {
        public static string AppPath { get { return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\"; } }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
