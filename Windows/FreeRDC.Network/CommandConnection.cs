using SharpRUDP;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandConnection
    {
        public RUDPConnection Connection { get; set; }

        private static JavaScriptSerializer _js = new JavaScriptSerializer();

        public void Server(string address, int port)
        {
            Connection.OnPacketReceived += EvtPacketReceived;
            Connection.Listen(address, port);
        }

        public void Client(string address, int port)
        {
            Connection.Connect(address, port);
        }

        private void EvtPacketReceived(RUDPPacket p)
        {
            Command c = _js.Deserialize<Command>(Encoding.ASCII.GetString(p.Data));
        }
    }
}
