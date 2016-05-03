using SharpRUDP;
using System.Net;

namespace FreeRDC.Network.Master
{
    public class MasterService : CommandBase
    {
        private MasterCore master;

        public MasterService()
        {
            RUDPLogger.LogLevel = RUDPLogger.RUDPLoggerLevel.None;
            master = new MasterCore(this);
        }

        public override void OnClientConnect(IPEndPoint ep)
        {
            base.OnClientConnect(ep);
            master.AddConnection(ep);
            Server.SendCommand(ep, new RDCCommand()
            {
                Command = RDCCommandType.MASTER_AUTH
            });
        }

        // TODO: OnClientDisconnected
        /*
        public override void OnClientDisconnected(Peer client)
        {
            base.OnClientDisconnected(client);
            master.RemoveConnection(client);
        }
        */

        public void SetNotice(string notice)
        {
            master.SetNotice(notice);
        }

        public override void OnCommandReceived(IPEndPoint source, RDCCommand cmd)
        {
            base.OnCommandReceived(source, cmd);
            switch(cmd.Command)
            {
                case RDCCommandType.MASTER_AUTH_HOST:
                    MasterCore.ConnectedHost host = master.AddHost(source, (string)cmd.Data);
                    Server.SendCommand(source, new RDCCommand()
                    {
                        Command = RDCCommandType.MASTER_AUTH_HOST_OK,
                        Data = new RDCCommandPackets.IntroducerPacket()
                        {
                            HostID = host.HostID,
                            Address = source.Address.ToString(),
                            Port = source.Port
                        }
                    });
                    break;

                case RDCCommandType.MASTER_AUTH_CLIENT:
                    MasterCore.ConnectedClient client = master.AddClient(source, (string)cmd.Data);
                    Server.SendCommand(source, new RDCCommand()
                    {
                        Command = RDCCommandType.MASTER_AUTH_CLIENT_OK,
                        Data = client.ClientID
                    });
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT:
                    IPEndPoint holeHost = master.FindHostByID(cmd.DestinationID);
                    IPEndPoint holeClient = master.FindClientByID(cmd.SourceID);
                    if (holeHost != null && holeClient != null)
                    {
                        // TODO: Master should send host client ID requesting to authenticate.
                        // Host should store ID and then mark it as an allowed client.
                        if (master.FindClientByConnection(holeClient) == cmd.SourceID) // Avoid impersonation
                            Server.SendCommand(holeClient, new RDCCommand()
                            {
                                Command = RDCCommandType.MASTER_CLIENT_CONNECT_OK,
                                SourceID = cmd.DestinationID,
                                Data = new RDCCommandPackets.IntroducerPacket()
                                {
                                    HostID = cmd.DestinationID,
                                    Address = holeHost.Address.ToString(),
                                    Port = holeHost.Port
                                }
                            });
                        else
                            Server.SendCommand(source, new RDCCommand()
                            {
                                Command = RDCCommandType.MASTER_CLIENT_CONNECT_ERROR,
                                SourceID = cmd.DestinationID,
                                Data = "Unknown host"
                            });
                    }
                    else
                        Server.SendCommand(source, new RDCCommand()
                        {
                            Command = RDCCommandType.MASTER_CLIENT_CONNECT_ERROR,
                            SourceID = cmd.DestinationID,
                            Data = "Unknown host"
                        });
                    break;
            }
        }
    }
}

