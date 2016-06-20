using SharpRUDP;
using SharpRUDP.Serializers;
using System;
using System.Net;
using System.Threading;

namespace FreeRDC.Network
{
    public class CommandConnection
    {
        public bool IsServer { get; set; }
        public RUDPConnection Connection { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }

        public delegate void dlgConnectionEvent(IPEndPoint ep);
        public delegate void dlgCommandEvent(IPEndPoint ep, CommandContainer cmd);
        public event dlgCommandEvent OnCommandReceived;
        public event dlgConnectionEvent OnConnected;

        private CommandSerializer _cs = new CommandSerializer();

        public CommandConnection()
        {
            Connection = new RUDPConnection();
            // Connection.DebugEnabled = true;
            // Connection.SerializeMode = RUDPSerializeMode.Binary;
            Connection.OnSocketError += (IPEndPoint ep, Exception ex) => {
                Console.WriteLine("Socket error");
                Thread.Sleep(5000);
                if (IsServer)
                    Server(Connection.Address, Connection.Port);
                else
                    Client(Connection.Address, Connection.Port);
            };
            Connection.OnConnected += (IPEndPoint ep) => { OnConnected?.Invoke(ep); };
        }

        public void Server(string address, int port)
        {
            IsServer = true;
            Connection.OnPacketReceived += EvtPacketReceived;
            Connection.Listen(address, port);
        }

        public void Client(string address, int port)
        {
            IsServer = false;
            Connection.OnPacketReceived += EvtPacketReceived;
            Connection.Connect(address, port);
        }

        public void SendCommand(IPEndPoint destination, string id, object cmd, Action EvtCommandSent = null)
        {
            CommandContainer command = new CommandContainer() { ID = id, Type = (byte)(ECommandType)Enum.Parse(typeof(ECommandType), cmd.GetType().Name), Command = _cs.Serialize(cmd) };
            byte[] data = _cs.Serialize(command);
            Console.WriteLine("SEND -> {0}|{1}", (ECommandType)command.Type, cmd);
            Connection.Send(destination, data);
        }

        private void EvtPacketReceived(RUDPPacket p)
        {
            CommandContainer cmd = _cs.DeserializeAs<CommandContainer>(p.Data);
            Console.WriteLine("RECV <- {0}|{1}", (ECommandType)cmd.Type, cmd);
            OnCommandReceived?.Invoke(p.Src, cmd);
        }
    }
}
    