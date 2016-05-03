using FreeRDC.Common.Hardware;
using SharpRUDP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace FreeRDC.Network.Host
{
    public class HostService : CommandBase
    {
        public class ConnectedClient
        {
            public string ClientID { get; set; }
            public IPEndPoint Connection { get; set; }
        }

        public delegate void VoidDelegate();
        public delegate void ConnectionDelegate(IPEndPoint client);
        public delegate void dMasterConnected(string hostId);
        public delegate void dClientDisconnected(string clientId);
        public delegate void NoticeDelegate(string notice);
        public event VoidDelegate OnInitializing;
        public event VoidDelegate OnInitialized;
        public event VoidDelegate OnMasterConnecting;
        public event dMasterConnected OnMasterConnected;
        public event VoidDelegate OnClientConnected;
        public event dClientDisconnected OnClientDisconnected;
        public event VoidDelegate OnMasterConnectionError;
        public event NoticeDelegate OnMasterNotice;

        public string HostID { get; set; }
        public string Password { get; set; }
        public string Fingerprint { get; set; }
        public List<ConnectedClient> Clients { get; set; }

        private bool _isInitialized = false;
        private object _mutex = new object();
        private RDCScreenCapture _screencap = new RDCScreenCapture();

        public void Init()
        {
            Clients = new List<ConnectedClient>();
            OnInitializing?.Invoke();
            if(!_isInitialized)
                Fingerprint = HWID.GenerateFingerprint();
            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        public override void Connect(string hostname, int port)
        {
            OnMasterConnecting?.Invoke();
            base.Connect(hostname, port);
        }

        // TODO: OnDisconnected
        /*        
        public override void OnDisconnected(Peer client)
        {
            ConnectedClient c = Clients.Where(x => x.Connection == client).SingleOrDefault();
            if (c != null)
            {
                OnClientDisconnected?.Invoke(c.ClientID);
                Clients.Remove(c);
            }
            base.OnDisconnected(client);
        }
        */

        // TOOD: ConnectTimeout
        /*
        public override void ConnectTimeout()
        {
            base.ConnectTimeout();
            OnMasterConnectionError?.Invoke();
        }
        */

        public override void OnCommandReceived(IPEndPoint source, RDCCommand cmd)
        {
            base.OnCommandReceived(source, cmd);
            switch (cmd.Command)
            {
                case RDCCommandType.MASTER_AUTH:
                    Client.SendCommand(source, new RDCCommand()
                    {
                        Command = RDCCommandType.MASTER_AUTH_HOST,
                        Data = Fingerprint
                    });
                    break;

                case RDCCommandType.MASTER_AUTH_HOST_OK:
                    RDCCommandPackets.IntroducerPacket outsideEndpoint = cmd.CastDataAs<RDCCommandPackets.IntroducerPacket>();
                    HostID = outsideEndpoint.HostID;
                    Listen(outsideEndpoint.Address, outsideEndpoint.Port);
                    OnMasterConnected?.Invoke(HostID);
                    break;

                case RDCCommandType.MASTER_NOTICE:
                    OnMasterNotice?.Invoke((string)cmd.Data);
                    break;

                case RDCCommandType.HOST_CONNECT:
                    string clientId = cmd.SourceID;
                    string pass = (string)cmd.Data;
                    if (pass == Password)
                    {
                        OnClientConnected?.Invoke();
                        Server.SendCommand(source, new RDCCommand()
                        {
                            Command = RDCCommandType.HOST_CONNECT_OK
                        });
                        lock(_mutex)
                        {
                            if (Clients.Where(x => x.ClientID == clientId).SingleOrDefault() == null)
                                Clients.Add(new ConnectedClient()
                                {
                                    ClientID = clientId,
                                    Connection = source
                                });
                        }              
                    }
                    else
                        Server.SendCommand(source, new RDCCommand()
                        {
                            Command = RDCCommandType.HOST_CONNECT_ERROR
                        });
                    break;

                case RDCCommandType.HOST_GETINFO:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    Server.SendCommand(source, new RDCCommand()
                    {
                        Command = RDCCommandType.HOST_NEWINFO,
                        SourceID = HostID,
                        Data = new RDCCommandPackets.HostInfoPacket()
                        {
                            ScreenWidth = Screen.PrimaryScreen.Bounds.Width,
                            ScreenHeight = Screen.PrimaryScreen.Bounds.Height,
                        }
                    });
                    break;

                case RDCCommandType.HOST_SCREEN_REFRESH:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    MemoryStream ms = _screencap.Capture3();
                    Server.SendCommand(source, new RDCCommand()
                    {
                        Command = RDCCommandType.HOST_SCREEN_REFRESH_OK,
                        SourceID = HostID,
                        Buffer = ms.ToArray()
                    });
                    break;

                case RDCCommandType.HOST_MOUSE_MOVE:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    RDCCommandPackets.HostMouseEvent mouseEvent = cmd.CastDataAs<RDCCommandPackets.HostMouseEvent>();
                    RDCRemoteMouse.Move(mouseEvent.MouseX, mouseEvent.MouseY);
                    break;

                case RDCCommandType.HOST_MOUSE_DOWN:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    mouseEvent = mouseEvent = cmd.CastDataAs<RDCCommandPackets.HostMouseEvent>();
                    RDCRemoteMouse.Down(mouseEvent.MouseX, mouseEvent.MouseY, mouseEvent.Buttons);
                    break;

                case RDCCommandType.HOST_MOUSE_UP:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    mouseEvent = mouseEvent = cmd.CastDataAs<RDCCommandPackets.HostMouseEvent>();
                    RDCRemoteMouse.Up(mouseEvent.MouseX, mouseEvent.MouseY, mouseEvent.Buttons);
                    break;

                case RDCCommandType.HOST_KEY_DOWN:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    RDCCommandPackets.HostKeyEvent keyEvent = cmd.CastDataAs<RDCCommandPackets.HostKeyEvent>();
                    RDCRemoteKeyboard.Down((short)keyEvent.KeyCode, keyEvent.Shift);
                    break;

                case RDCCommandType.HOST_KEY_UP:
                    if (!ValidateClient(cmd.SourceID, source))
                        return;
                    keyEvent = cmd.CastDataAs<RDCCommandPackets.HostKeyEvent>();
                    RDCRemoteKeyboard.Up((short)keyEvent.KeyCode, keyEvent.Shift);
                    break;
            }
        }

        private bool ValidateClient(string clientID, IPEndPoint connection)
        {
            if (Clients.Where(x => x.ClientID == clientID && x.Connection.ToString() == connection.ToString()).SingleOrDefault() != null)
                return true;
            Server.SendCommand(connection, new RDCCommand()
            {
                Command = RDCCommandType.HOST_ERROR,
                SourceID = HostID
            });
            return false;
        }

        public void Shutdown()
        {
            Disconnect();
        }
    }
}
