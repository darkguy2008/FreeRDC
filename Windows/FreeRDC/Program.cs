using System;
using System.IO;
using System.Reflection;

namespace FreeRDC
{
    static class Program
    {
        public static App app = new App();
        public static string AppPath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            app.Init(args[0]);
        }
    }
}
