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
        public IPEndPoint RemoteEndPoint { get; set; }

        public delegate void dlgConnectionEvent(IPEndPoint ep);
        public delegate void dlgCommandEvent(IPEndPoint ep, CommandSerializer serializer, CommandContainer cmd);
        public event dlgCommandEvent OnCommandReceived;
        public event dlgConnectionEvent OnConnected;
        public event dlgConnectionEvent OnDisconnected;
        public event dlgConnectionEvent OnClientConnected;

        private CommandSerializer _cs = new CommandSerializer();

        public CommandConnection()
        {
            Connection = new RUDPConnection();
            Connection.DebugEnabled = false;
            Connection.SerializeMode = RUDPSerializeMode.Binary;
            Connection.OnConnected += (IPEndPoint ep) => { OnConnected?.Invoke(ep); };
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

        public void SendCommand(IPEndPoint destination, string tagFrom, string tagTo, object cmd, Action EvtCommandSent)
        {
            Console.WriteLine("SEND -> {0}", cmd);
            byte[] data = _cs.Serialize(new CommandContainer() { TagFrom = tagFrom, TagTo = tagTo, Type = (ECommandType)Enum.Parse(typeof(ECommandType), cmd.GetType().Name, true), Command = _cs.Serialize(cmd) });
            Connection.InitializePacket(destination, data, (RUDPPacket p) => { EvtCommandSent?.Invoke(); }).Send();
        }

        private void EvtPacketReceived(RUDPPacket p)
        {
            CommandContainer cmd = _cs.DeserializeAs<CommandContainer>(p.Data);
            Console.WriteLine("RECV <- {0}", cmd);
            OnCommandReceived?.Invoke(p.Src, _cs, cmd);
        }
    }
}
