using ENet;

namespace FreeRDC.Network.Master
{
    public class MasterService : CommandServer
    {
        private MasterCore master;

        public MasterService()
        {
            master = new MasterCore();
        }

        public override void OnClientConnected(Peer client)
        {
            base.OnClientConnected(client);
            master.AddConnection(client);
            SendCommand(client, new RDCCommand()
            {
                Channel = RDCCommandChannel.Auth,
                Command = RDCCommandType.MASTER_AUTH
            });
        }

        public override void OnClientDisconnected(Peer client)
        {
            base.OnClientDisconnected(client);
            master.RemoveConnection(client);
        }

        public void SetNotice(string notice)
        {
            master.SetNotice(notice);
        }

        public override void OnCommandReceived(Event evt, RDCCommand cmd)
        {
            base.OnCommandReceived(evt, cmd);
            switch(cmd.Command)
            {
                case RDCCommandType.MASTER_AUTH_HOST:
                    MasterCore.ConnectedHost host = master.AddHost(evt.Peer, (string)cmd.Data);
                    SendCommand(evt.Peer, new RDCCommand()
                    {
                        Channel = RDCCommandChannel.Auth,
                        Command = RDCCommandType.MASTER_AUTH_HOST_OK,
                        Data = host.HostID
                    });
                    break;

                case RDCCommandType.MASTER_AUTH_CLIENT:
                    MasterCore.ConnectedClient client = master.AddClient(evt.Peer, (string)cmd.Data);
                    SendCommand(evt.Peer, new RDCCommand()
                    {
                        Channel = RDCCommandChannel.Auth,
                        Command = RDCCommandType.MASTER_AUTH_CLIENT_OK,
                        Data = client.ClientID
                    });
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT:
                    Peer? holeHost = master.FindHostByID(cmd.DestinationID);
                    Peer? holeClient = master.FindClientByID(cmd.SourceID);
                    if (holeHost.HasValue && holeClient.HasValue)
                    {
                        // TODO: Master should send host client ID requesting to authenticate.
                        // Host should store ID and then mark it as an allowed client.
                        if (master.FindClientByConnection(holeClient.Value) == cmd.SourceID) // Avoid impersonation
                            SendCommand(holeClient.Value, new RDCCommand()
                            {
                                Channel = RDCCommandChannel.Auth,
                                Command = RDCCommandType.MASTER_CLIENT_CONNECT_OK,
                                SourceID = cmd.DestinationID,
                                Data = new RDCCommandPackets.IntroducerPacket()
                                {
                                    HostID = cmd.DestinationID,
                                    Address = holeHost.Value.GetRemoteAddress().ToString().Split(':')[0],
                                    Port = int.Parse(holeHost.Value.GetRemoteAddress().ToString().Split(':')[1])
                                }
                            });
                        else
                            SendCommand(evt.Peer, new RDCCommand()
                            {
                                Channel = RDCCommandChannel.Auth,
                                Command = RDCCommandType.MASTER_CLIENT_CONNECT_ERROR,
                                SourceID = cmd.DestinationID,
                                Data = "Unknown host"
                            });
                    }
                    else
                        SendCommand(evt.Peer, new RDCCommand()
                        {
                            Channel = RDCCommandChannel.Auth,
                            Command = RDCCommandType.MASTER_CLIENT_CONNECT_ERROR,
                            SourceID = cmd.DestinationID,
                            Data = "Unknown host"
                        });
                    break;
            }
        }
    }
}

