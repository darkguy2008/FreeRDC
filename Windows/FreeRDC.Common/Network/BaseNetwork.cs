using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FreeRDC.Common.Network
{
    public class BaseNetwork
    {
        public BinaryFormatter binFmt = new BinaryFormatter();

        public void SendCommand(BinaryWriter bw, String cmd, object arguments)
        {
            MemoryStream ms = new MemoryStream();
            CommandStruct data = new CommandStruct() { Command = cmd };
            if (arguments != null)
                data.Payload = arguments;
            binFmt.Serialize(ms, data);
            Debug.WriteLine("Sending " + ms.Length + " bytes");
            bw.Write(ms.ToArray());
        }
    }
}
