using SharpRUDP;
using System;

namespace FreeRDC.Network
{
    public class CommandConnection
    {
        public static CommandSerializer Serializer = new CommandSerializer();

        public bool IsServer { get { return Connection.IsServer; } }
        public bool IsAlive { get { return Connection.IsAlive; } }
        public RUDPConnection Connection { get; set; }
        public string ChannelName { get; set; }

        public delegate void dConnectionEvent(RUDPChannel channel);
        public delegate void dCommandEvent(RUDPChannel channel, CommandContainer command);

        public event dConnectionEvent OnConnected;
        public event dConnectionEvent OnIncomingConnection;
        public event dCommandEvent OnCommandReceived;

        public void Connect(string masterAddress, int masterPort, string channelName)
        {
            ChannelName = channelName;
            Connection = new RUDPConnection();
            Connection.Create(false, masterAddress, masterPort);
            Connection.OnConnected += Connection_OnConnected;
            Connection.OnChannelAssigned += Connection_OnChannelAssigned;
            Connection.OnPacketReceived += Connection_OnPacketReceived;
            Connection.RequestChannel(Connection.RemoteEndpoint, channelName);
        }

        public void Listen(string address, int port)
        {
            Connection = new RUDPConnection();
            Connection.Create(true, address, port);
            Connection.OnConnected += Connection_OnConnected;
            Connection.OnIncomingConnection += Connection_OnIncomingConnection;
            Connection.OnPacketReceived += Connection_OnPacketReceived;
        }


        private void Connection_OnConnected(RUDPChannel channel)
        {
            RUDPConnection.Trace("Channel {0} connected! calling {1}", channel.Name, OnConnected);
            OnConnected?.Invoke(channel);
        }

        private void Connection_OnIncomingConnection(RUDPChannel channel)
        {
            RUDPConnection.Trace("Channel {0} for {1} incoming!", channel.Name, ChannelName);
            OnIncomingConnection?.Invoke(channel);
        }

        private void Connection_OnChannelAssigned(RUDPChannel channel)
        {
            RUDPConnection.Trace("Channel {0} for {1} assigned!", channel.Name, ChannelName);
            if (channel.Name == ChannelName)
                channel.Connect();
        }

        private void Connection_OnPacketReceived(RUDPChannel channel, RUDPPacket p)
        {
            CommandContainer cmd = Serializer.DeserializeAs<CommandContainer>(p.Data);
            RUDPConnection.Debug("PKT RECV <- {0}: {1}", channel.Name, (ECommandType)cmd.Type);
            OnCommandReceived?.Invoke(channel, cmd);
        }

        public void SendCommand(RUDPChannel channel, string tag, object cmd, Action EvtCommandSent = null)
        {
            CommandContainer command = new CommandContainer() { Tag = tag, Type = (byte)(ECommandType)Enum.Parse(typeof(ECommandType), cmd.GetType().Name), Command = Serializer.Serialize(cmd) };
            byte[] data = Serializer.Serialize(command);
            RUDPConnection.Debug("PKT SEND -> {0}: {1}", (ECommandType)command.Type, cmd);
            channel.SendData(data);
            EvtCommandSent?.Invoke();
        }
    }
}
