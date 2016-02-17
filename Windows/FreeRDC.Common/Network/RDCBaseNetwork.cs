using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FreeRDC.Common.Network
{
    public class RDCBaseNetwork
    {
        public BinaryFormatter binFmt = new BinaryFormatter();

        public void SendCommand(BinaryWriter bw, RDCCommandType cmd, object arguments)
        {
            MemoryStream ms = new MemoryStream();
            RDCCommandStruct data = new RDCCommandStruct() { Command = cmd };
            if (arguments != null)
                data.Payload = arguments;
            binFmt.Serialize(ms, data);
            Debug.WriteLine("Sending " + ms.Length + " bytes");
            bw.Write(ms.ToArray());
        }
    }
}
