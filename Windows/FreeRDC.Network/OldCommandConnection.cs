using SharpRUDP;
using System;
using System.Net;
using System.Threading;

namespace FreeRDC.Network
{
    public class CommandConnection
    {
        public bool IsServer { get; set; }
        public RUDPConnection Connection { get; set; }
        public RUDPEndpoint RemoteEndpoint { get; set; }        

        public delegate void dlgConnectionEvent(int channelId, string channelName, IPEndPoint connectionEndPoint);
        public delegate void dlgCommandEvent(IPEndPoint ep, CommandContainer cmd);
        public delegate void dlgChannelAssigned(int channelId, string channelName);
        public event dlgCommandEvent OnCommandReceived;
        public event dlgConnectionEvent OnConnected;
        public event dlgChannelAssigned OnChannelAssigned;

        private CommandSerializer _cs = new CommandSerializer();

        public CommandConnection()
        {
            Connection = new RUDPConnection();
            Connection.OnConnection += (int channelId, string channelName, IPEndPoint connectionEndPoint) => { OnConnected?.Invoke(channelId, channelName, connectionEndPoint); };
        }

        public void Server(string address, int port)
        {
            IsServer = true;
            Connection.OnPacketReceived += EvtPacketReceived;
            Connection.Create(true, address, port);
        }

        public void Client(string address, int port, string channelName)
        {
            IsServer = false;
            Connection.OnPacketReceived += EvtPacketReceived;            
            RemoteEndpoint = Connection.Create(false, address, port);
            RemoteEndpoint.OnChannelAssigned += RemoteEndpoint_OnChannelAssigned;
            RemoteEndpoint.RequestChannel(channelName);
        }

        private void RemoteEndpoint_OnChannelAssigned(int channelId, string channelName)
        {
            OnChannelAssigned?.Invoke(channelId, channelName);
            RemoteEndpoint.GetChannel(channelName).Connect();
        }

        public void SendCommand(IPEndPoint destination, string channelName, string tag, object cmd, Action EvtCommandSent = null)
        {
            CommandContainer command = new CommandContainer() { Tag = tag, Type = (byte)(ECommandType)Enum.Parse(typeof(ECommandType), cmd.GetType().Name), Command = _cs.Serialize(cmd) };
            byte[] data = _cs.Serialize(command);
            //Console.WriteLine("SEND -> {0}|{1}", (ECommandType)command.Type, cmd);
            Connection.SendData(destination, channelName, data);
            EvtCommandSent?.Invoke();
        }

        private void EvtPacketReceived(RUDPPacket p)
        {
            CommandContainer cmd = _cs.DeserializeAs<CommandContainer>(p.Data);
            //Console.WriteLine("RECV <- {0}|{1}", (ECommandType)cmd.Type, cmd);
            OnCommandReceived?.Invoke(p.Src, cmd);
        }

        public void Shutdown()
        {
            // Connection.Disconnect();
        }
    }
}
    