using System.Collections.Generic;
using System.IO;

namespace FreeRDC.Common.IO
{
    public static class INIFile
    {
        public static Dictionary<string, Dictionary<string, string>> Read(string iniFile)
        {
            Dictionary<string, Dictionary<string, string>> rv = new Dictionary<string, Dictionary<string, string>>();

            string lastKey = string.Empty;
            foreach (string l in File.ReadAllLines(iniFile))
            {
                if (l.StartsWith(";") || l.StartsWith("#"))
                    continue;
                if (l.StartsWith("["))
                {
                    lastKey = l.Substring(l.IndexOf('[') + 1, l.LastIndexOf(']') - 1);
                    rv[lastKey] = new Dictionary<string, string>();
                }
                else
                    if (l.Contains("="))
                    rv[lastKey][l.Substring(0, l.IndexOf('='))] = l.Substring(l.IndexOf('=') + 1);
            }

            return rv;
        }

        public static void Save(this Dictionary<string, Dictionary<string, string>> ini, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false))
                foreach (string key in ini.Keys)
                {
                    sw.WriteLine("[" + key + "]");
                    foreach (KeyValuePair<string, string> kvp in ini[key])
                        sw.WriteLine(kvp.Key + "=" + kvp.Value);
                    sw.WriteLine();
                }
        }
    }
}
