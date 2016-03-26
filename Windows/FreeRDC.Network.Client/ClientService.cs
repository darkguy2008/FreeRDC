using ENet;
using FreeRDC.Common.Hardware;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public event VoidDelegate OnInitializing;
        public event VoidDelegate OnInitialized;
        public event VoidDelegate OnConnectingToMaster;
        public event VoidDelegate OnConnectedToMaster;

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
            SendCommand(Connection, RDCCommandChannel.Auth, RDCCommandType.MASTER_CLIENT_CONNECT, hostId);
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
                    SendCommand(Connection, RDCCommandChannel.Auth, RDCCommandType.MASTER_AUTH_CLIENT, null);
                    break;

                case RDCCommandType.MASTER_AUTH_CLIENT_OK:
                    ClientID = cmd.StringData;
                    OnConnectedToMaster?.Invoke();
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT_OK:
                    string address = Encoding.UTF8.GetString(cmd.ByteData);
                    string ip = address.Split(':')[0];
                    int port = int.Parse(address.Split(':')[1]);
                    string host = cmd.StringData;
                    HostConnection h = new HostConnection(ClientID, host);
                    if (OnHostConnecting != null) h.OnHostConnecting += OnHostConnecting;
                    if (OnHostConnected != null) h.OnHostConnected += OnHostConnected;
                    if (OnHostError != null) h.OnHostError += OnHostError;
                    lock (_mutex)
                    {
                        HostConnections.Add(h);
                        h = HostConnections.Last();
                    }
                    h.Connect(ip, port, ConnectionPassword);
                    ConnectionPassword = string.Empty;
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT_ERROR:
                    OnHostConnectionError?.Invoke(cmd.StringData);
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
