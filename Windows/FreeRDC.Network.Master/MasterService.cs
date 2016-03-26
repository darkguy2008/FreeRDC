using ENet;
using System.Text;

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
            SendCommand(client, RDCCommandChannel.Auth, RDCCommandType.MASTER_AUTH, null);
        }

        public override void OnCommandReceived(Event evt, RDCCommand cmd)
        {
            base.OnCommandReceived(evt, cmd);
            switch(cmd.Command)
            {
                case RDCCommandType.MASTER_AUTH_HOST:
                    MasterCore.ConnectedHost host = master.AddHost(evt.Peer, cmd.StringData);
                    SendCommand(evt.Peer, RDCCommandChannel.Auth, RDCCommandType.MASTER_AUTH_HOST_OK, host.HostID);
                    break;

                case RDCCommandType.MASTER_AUTH_CLIENT:
                    MasterCore.ConnectedClient client = master.AddClient(evt.Peer, cmd.StringData);
                    SendCommand(evt.Peer, RDCCommandChannel.Auth, RDCCommandType.MASTER_AUTH_CLIENT_OK, client.ClientID);
                    break;

                case RDCCommandType.MASTER_CLIENT_CONNECT:
                    Peer? holeHost = master.FindHostByID(cmd.StringData);
                    Peer holeClient = evt.Peer;
                    if (holeHost.HasValue)
                        SendCommand(holeClient, RDCCommandChannel.Auth, RDCCommandType.MASTER_CLIENT_CONNECT_OK, cmd.StringData, Encoding.UTF8.GetBytes(holeHost.Value.GetRemoteAddress().ToString()));
                    else
                        SendCommand(evt.Peer, RDCCommandChannel.Auth, RDCCommandType.MASTER_CLIENT_CONNECT_ERROR, cmd.StringData, Encoding.UTF8.GetBytes("Unknown host"));
                    break;
            }
        }
    }
}

