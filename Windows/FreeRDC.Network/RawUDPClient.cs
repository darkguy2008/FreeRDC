using ENet;
using System.Threading;

namespace FreeRDC.Network
{
    public class RawUDPClient
    {
        public Host Client;
        public Peer Connection;
        public bool IsConnected { get { return Connection.State >= PeerState.Connecting && Connection.State <= PeerState.Connected; } }

        public delegate void onConnected(Peer connection);
        public delegate void onDataReceived(Event evt, byte[] data);
        public event onDataReceived OnDataReceived;
        public event onConnected OnConnected;

        public RawUDPClient()
        {
            Client = new Host();
            Client.Initialize(null, 4095);
        }

        public void Connect(string hostname, int port)
        {
            new Thread(() =>
            {
                Connection = Client.Connect(hostname, port, 0, 4);
                while (IsConnected)
                {
                    Event evt;
                    if(Client.Service(10, out evt))
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
                            }
                        } while (Client.CheckEvents(out evt));
                    }
                }
            }).Start();                
        }

        public void Disconnect()
        {
            Connection.DisconnectNow(-1);
            while (IsConnected)
                Thread.Sleep(100);
        }
    }
}
