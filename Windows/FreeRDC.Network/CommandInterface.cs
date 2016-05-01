using SharpRUDP;
using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandInterface : RUDPConnection
    {
        internal JavaScriptSerializer _js = new JavaScriptSerializer();

        public void SendCommand(IPEndPoint destination, RDCCommand cmd)
        {
            Console.WriteLine("SEND -> " + _js.Serialize(cmd));
            Send(destination, RUDPPacketType.DAT, Encoding.UTF8.GetBytes(_js.Serialize(cmd)));
        }
    }
}