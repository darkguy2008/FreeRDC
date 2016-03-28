using ENet;
using FreeRDC.Common.Hardware;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FreeRDC.Network.Client
{
    public class ClientService : CommandClient
    {
        public Peer Connection;
        public object _mutex = new object();
        public List<HostConnection> HostConnections = new List<HostConnection>();

        public string ClientID { get; set; }
        public string Fingerprint { get; set; }
        public string ConnectionPassword { get; set; }

        public delegate void VoidDelegate();
        public delegate void NoticeDelegate(string notice);
        public event VoidDelegate OnInitializing;
        public event VoidDelegate OnInitialized;
        public event VoidDelegate OnConnectingToMaster;
        public event VoidDelegate OnConnectedToMaster;
        public event NoticeDelegate OnMasterNotice;

        public event HostConnection.dHostConnection OnHostConnected;
        public event HostConnection.dHostConnection OnHostConnecting;
        public event HostConnection.dHostConnection OnHostConnectionError;
        public event HostConnection.dHostConnection OnHostError;

        public void Init()
        {
            OnInitializing?.Invoke();
            Fingerprint = HWID.GenerateFingerprint();
            OnInitialized?.Invoke();
        }

        public override void Connect(string hostname, int port)
        {
            OnConnectingToMaster?.Invoke();
            base.Connect(hostname, port);
        }

        public void ConnectHost(string hostId, string password)
        {
            ConnectionPassword = password;
            SendCommand(Connection, new RDCCommand()
            {
                Channel = RDCCommandChannel.Auth,
                Command = RDCCommandType.MASTER_CLIENT_CONNECT,
                SourceID = ClientID,
                DestinationID = hostId
            });
        }

        public void RemoveConnections(string hostId)
        {
            lock(_mutex) HostConnections.RemoveAll(x => x.HostID == hostId);
        }

        public override void OnConnected(Peer client)
        {
            base.OnConnected(client);
            Connection = client;
        }

        public override void OnCommandReceived(Event evt, RDCCommand cmd)
        {
            base.OnCommandReceived(evt, cmd);
            switch (cmd.Command)
            {
                case RDCCommandType.MASTER_AUTH:
                    SendCommand(Connection, new RDCCommand()
                    {
                        Channel = RDCCommandChannel.Auth,
                        Command = RDCCommandType.MASTER_AUTH_CLIENT
                    });
                    break;

                case RDCCommandType.MASTER_AUTH_CLIENT_OK:
                    ClientID = (string)cmd.Data;
                    OnConnectedToMaster?.Invoke();
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT_OK:
                    RDCCommandPackets.IntroducerPacket ipData = cmd.CastDataAs<RDCCommandPackets.IntroducerPacket>();
                    HostConnection h = new HostConnection(ClientID, ipData.HostID);
                    if (OnHostConnecting != null) h.OnHostConnecting += OnHostConnecting;
                    if (OnHostConnected != null) h.OnHostConnected += OnHostConnected;
                    if (OnHostError != null) h.OnHostError += OnHostError;
                    lock (_mutex)
                    {
                        HostConnections.Add(h);
                        h = HostConnections.Last();
                    }
                    h.Connect(ipData.Address, ipData.Port, ConnectionPassword);
                    ConnectionPassword = string.Empty;
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT_ERROR:
                    OnHostConnectionError?.Invoke((string)cmd.Data);
                    break;

                case RDCCommandType.MASTER_NOTICE:
                    OnMasterNotice?.Invoke((string)cmd.Data);
                    break;
            }
        }

        public void Shutdown()
        {
            Connection.DisconnectNow(-1);
            while (IsConnected)
                Thread.Sleep(100);
        }
    }
}
