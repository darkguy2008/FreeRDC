using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Services.Master;
using FreeRDC.Common.IO;
using System.IO;
using System.Reflection;

namespace FreeRDC.Master
{
    class Program
    {
        public static string AppPath { get { return new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName + "\\"; } }
        public static Dictionary<string, Dictionary<string, string>> Config;
        public static string ConfigFilename { get; set; }

        static void Main(string[] args)
        {
            ConfigFilename = AppPath + "Config.ini";
            Config = INIFile.Read(ConfigFilename);

            MasterService master = new MasterService(Config["Database"]["ConnectionString"]);
            master.Start();

            Console.WriteLine("Server started");
            Console.ReadKey();

            master.Stop();

            Console.WriteLine("Server stopped");
            Console.ReadKey();

        }
    }
}
