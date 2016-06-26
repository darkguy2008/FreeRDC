using System;
using System.Collections.Generic;
using System.IO;

namespace FreeRDC.Common.IO
{
    public class INIFile
    {
        private string _filename;
        private Dictionary<string, Dictionary<string, string>> _config = new Dictionary<string, Dictionary<string, string>>();

        public string GetValue(string section, string key)
        {
            if (_config.ContainsKey(section))
                if (_config[section].ContainsKey(key))
                    return _config[section][key];
                else
                    throw new ArgumentNullException(section + "|" + key, "Section/Key pair not found in config file");
            else
                throw new ArgumentNullException(section, "Section not found in config file");
        }

        public void SetValue(string section, string key, string value)
        {
            if (!_config.ContainsKey(section))
                _config[section] = new Dictionary<string, string>();
            _config[section][key] = value;
        }

        public void Read(string iniFile)
        {
            _filename = iniFile;
            string lastKey = string.Empty;
            foreach (string l in File.ReadAllLines(iniFile))
            {
                if (l.StartsWith(";") || l.StartsWith("#"))
                    continue;
                if (l.StartsWith("["))
                {
                    lastKey = l.Substring(l.IndexOf('[') + 1, l.LastIndexOf(']') - 1);
                    _config[lastKey] = new Dictionary<string, string>();
                }
                else
                    if (l.Contains("="))
                    _config[lastKey][l.Substring(0, l.IndexOf('='))] = l.Substring(l.IndexOf('=') + 1);
            }
        }

        public void Save()
        {
            Save(_filename);
        }
        public void Save(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false))
                foreach (string key in _config.Keys)
                {
                    sw.WriteLine("[" + key + "]");
                    foreach (KeyValuePair<string, string> kvp in _config[key])
                        sw.WriteLine(kvp.Key + "=" + kvp.Value);
                    sw.WriteLine();
                }
        }
    }
}
