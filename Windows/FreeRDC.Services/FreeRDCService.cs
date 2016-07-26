using FreeRDC.Network;
using FreeRDC.Services.Client;
using FreeRDC.Services.Host;
using FreeRDC.Services.Master;
using SharpRUDP;
using System;
using System.Collections.Generic;

namespace FreeRDC.Services
{
    public class FreeRDCService
    {
        private CommandConnection _connection;
        public CommandConnection Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
                _master.Connection = _connection;
                _host.HostConnection = _connection;
                _host.Master = _master;
            }
        }

        public string AssignedTag { get { return _master.AssignedTag; } }
        public string Fingerprint { get { return _master.Fingerprint; } }
        public string HostPassword { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }

        public delegate void dlgConnected();
        public delegate void dlgAuthenticated(string tag, string address);
        public delegate void dlgIncomingConnection(Commands.INTRODUCER cmdIntroducer);
        public delegate void dlgHostConnection(CommandContainer cmd, Commands.INTRODUCER introducer);
        public event dlgConnected OnConnectedToMaster;
        public event dlgAuthenticated OnAuthenticatedToMaster;
        public event dlgHostConnection OnConnectedToHost;
        public event dlgIncomingConnection OnIncomingClientConnection;

        private RUDPChannel _masterChannel;

        private MasterService _master = new MasterService();
        private HostService _host = new HostService();

        public FreeRDCService()
        {
            _master.OnAuthenticated += (string tag, string address) => { OnAuthenticatedToMaster?.Invoke(tag, address); };
            _master.OnHostConnected += (CommandContainer cmd, Commands.INTRODUCER introducer) => { OnConnectedToHost?.Invoke(cmd, introducer); };
            _master.OnIncomingConnection += (Commands.INTRODUCER cmdIntroducer) => { OnIncomingClientConnection?.Invoke(cmdIntroducer); };
            RegisterCommandService(_master);
            RegisterCommandService(_host);
        }

        public void Init()
        {
            _master.Init();
            _host.Init();
        }

        private List<ICommandService> _services = new List<ICommandService>();
        public void RegisterCommandService(ICommandService svc)
        {
            _services.Add(svc);
        }
        public void UnregisterCommandService(ICommandService svc)
        {
            _services.Remove(svc);
        }

        public void Start()
        {
            Connection.OnConnected += Connection_OnConnected;
            Connection.OnIncomingConnection += Connection_OnConnection;
            Connection.OnCommandReceived += Connection_OnCommandReceived;
            Connection.Connect(Address, Port, "FreeRDCMaster");
        }

        public void ConnectToHost(string tag)
        {
            Connection.SendCommand(_masterChannel, tag, new Commands.CLIENT_CONNECTIONREQUEST());
        }

        private void Connection_OnConnected(RUDPChannel channel)
        {
            RUDPConnection.Debug("ONCONNECTED!!! {0}", channel);
            if (channel.Name == "FreeRDCMaster")
            {
                _masterChannel = channel;
                Connection.SendCommand(channel, null, new Commands.AUTH() { Fingerprint = Fingerprint }, () =>
                {
                    Console.WriteLine("Identifying with fingerprint {0}...", Fingerprint);
                    OnConnectedToMaster?.Invoke();
                });
            }
            else
                foreach (ICommandService svc in _services)
                {
                    RUDPConnection.Trace("Connected event for {0}", svc);
                    svc.Connected(channel);
                }
        }

        private void Connection_OnConnection(RUDPChannel channel)
        {
            RUDPConnection.Debug("ONCONNECTION!!! {0}", channel);
            foreach (ICommandService svc in _services)
                svc.Connection(channel);
        }

        private void Connection_OnCommandReceived(RUDPChannel channel, CommandContainer cmd)
        {
            RUDPConnection.Debug("CMD RECV <- {0}: {1}", channel.EndPoint, (ECommandType)cmd.Type);
            foreach (ICommandService svc in _services)
                svc.ProcessCommand(channel, cmd);
        }
    }
}
