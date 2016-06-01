using SharpRUDP;
using SharpRUDP.Serializers;
using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace FreeRDC.Network
{
    public class CommandConnection
    {
        public RUDPConnection Connection { get; set; }

        public delegate void dlgCommandEvent(IPEndPoint ep, object cmd);
        public event dlgCommandEvent OnCommandReceived;

        private CommandSerializer _cs = new CommandSerializer();

        public CommandConnection()
        {
            Connection = new RUDPConnection();
            Connection.SerializeMode = RUDPSerializeMode.Binary;
        }

        public void Server(string address, int port)
        {            
            Connection.OnPacketReceived += EvtPacketReceived;
            Connection.Listen(address, port);
        }

        public void Client(string address, int port)
        {
            Connection.Connect(address, port);
        }

        private void SendCommand(IPEndPoint destination, object cmd, Action EvtCommandSent)
        {
            byte[] data = _cs.Serialize(cmd);
            Connection.InitializePacket(destination, data, (RUDPPacket p) => { EvtCommandSent?.Invoke(); });
        }

        private void EvtPacketReceived(RUDPPacket p)
        {
            object cmd = _cs.DeserializeAs<CommandContainer>(p.Data);
            OnCommandReceived?.Invoke(p.Src, cmd);
        }
    }
}
