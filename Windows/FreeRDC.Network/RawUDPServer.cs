using ENet;
using System.Threading;

namespace FreeRDC.Network
{
    public class RawUDPServer
    {
        public Host Listener;
        public bool IsListening { get; set; }

        public delegate void onClientConnected(Peer client);
        public delegate void onDataReceived(Event evt, byte[] data);
        public event onDataReceived OnDataReceived;
        public event onClientConnected OnClientConnected;

        public RawUDPServer()
        {
            IsListening = false;
            Listener = new Host();
        }

        public void Listen(int port)
        {
            Listener.InitializeServer(port, 4095);
            IsListening = true;
            new Thread(() =>
            {
                Peer connection = new Peer();
                while (IsListening)
                {
                    Event evt;
                    if (Listener.Service(10, out evt))
                    {
                        do
                        {
                            switch (evt.Type)
                            {
                                case EventType.Connect:
                                    connection = evt.Peer;
                                    OnClientConnected?.Invoke(connection);
                                    break;

                                case EventType.Receive:
                                    byte[] data = evt.Packet.GetBytes();
                                    evt.Packet.Dispose();
                                    OnDataReceived?.Invoke(evt, data);                                    
                                    break;
                            }
                        } while (Listener.CheckEvents(out evt));
                    }
                }

            }).Start();
        }
    }
}
