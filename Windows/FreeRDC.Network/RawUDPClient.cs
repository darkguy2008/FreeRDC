using ENet;
using System.Threading;

namespace FreeRDC.Network
{
    public class RawUDPClient
    {
        public Host Client;
        public Peer Connection;
        public bool IsConnected { get { return Connection.State == PeerState.Connected; } }
        public bool IsConnecting { get { return Connection.State > PeerState.Disconnected && Connection.State < PeerState.Connected; } }
        public int ConnectionTimeout { get; set; }

        public delegate void VoidDelegate();
        public delegate void onConnection(Peer connection);
        public delegate void onDataReceived(Event evt, byte[] data);
        public event onDataReceived OnDataReceived;
        public event onConnection OnConnected;
        public event onConnection OnDisconnected;
        public event VoidDelegate OnConnectionTimeout;
        private Thread thTimeout;
        private Thread thProcess;
        private Thread thKeepAlive;
        private bool keepAliveStarted;

        public RawUDPClient()
        {
            Client = new Host();
            Client.Initialize(null, 4095);
            ConnectionTimeout = 5000;
        }

        public void Connect(string hostname, int port)
        {
            thProcess = new Thread(() =>
            {
                Connection = Client.Connect(hostname, port, 0, 4);
                Connection.SetPingInterval(1000);
                Connection.SetTimeouts(5, 500, 1000);
                while (IsConnecting || IsConnected)
                {
                    if(!keepAliveStarted)
                    {
                        thKeepAlive = new Thread(() =>
                        {
                            while (IsConnecting || IsConnected)
                            {
                                try
                                {
                                    Packet p = new Packet();
                                    p.Initialize(new byte[] { 0x01 });
                                    Connection.Send(0, p);
                                }
                                catch(ENetException)
                                { }
                                Thread.Sleep(5000);
                            }
                        });
                        thKeepAlive.Start();
                        keepAliveStarted = true;
                    }

                    Event evt;
                    if (Client.Service(10, out evt))
                    {
                        do
                        {
                            switch (evt.Type)
                            {
                                case EventType.Connect:
                                    OnConnected?.Invoke(Connection);
                                    break;

                                case EventType.Receive:
                                    byte[] data = evt.Packet.GetBytes();
                                    evt.Packet.Dispose();
                                    OnDataReceived?.Invoke(evt, data);
                                    break;

                                case EventType.Disconnect:
                                    OnDisconnected?.Invoke(evt.Peer);
                                    break;
                            }
                        } while (Client.CheckEvents(out evt));
                    }
                }
            });
            thTimeout = new Thread(() =>
            {
                Thread.Sleep(ConnectionTimeout);
                if (!IsConnected)
                {
                    thProcess.Abort();
                    thKeepAlive.Abort();
                    OnConnectionTimeout?.Invoke();
                }
            });
            thTimeout.Start();
            thProcess.Start();
        }

        public void Disconnect()
        {
            Connection.DisconnectNow(-1);
            thKeepAlive.Abort();
            thTimeout.Abort();
            thProcess.Abort();
            keepAliveStarted = false;
            while (IsConnected)
                Thread.Sleep(100);
        }
    }
}
