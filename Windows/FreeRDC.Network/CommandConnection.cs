using SharpRUDP;
using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandConnection
    {
        public RUDPConnection Connection { get; set; }

        public delegate void dlgCommandEvent(IPEndPoint ep, Command cmd);
        public event dlgCommandEvent OnCommandReceived;

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
            OnCommandReceived?.Invoke(p.Src, c);
        }

        private void SendCommand(IPEndPoint destination, Command cmd, Action EvtCommandSent)
        {

        }
    }
}
