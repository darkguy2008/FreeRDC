using FreeRDC.Network;
using SharpRUDP;
using System;
using System.Linq;

namespace FreeRDC.Server.Master
{
    public class MasterServer : ICommandService
    {
        public CommandConnection Connection { get; set; }

        private MasterCore _master = new MasterCore();

        public void ProcessCommand(RUDPChannel channel, CommandContainer cmd)
        {
            switch ((ECommandType)cmd.Type)
            {
                case ECommandType.AUTH:
                    var cmdAuth = CommandConnection.Serializer.DeserializeAs<Commands.AUTH>(cmd.Command);
                    MasterCore.DeviceEntry device = _master.AddDevice(channel, cmdAuth.Fingerprint);
                    Connection.SendCommand(channel, "MASTER", new Commands.AUTH_OK() { AssignedID = device.AssignedID, EndpointAddress = device.Channel.EndPoint.ToString() });
                    break;
                case ECommandType.CLIENT_CONNECTIONREQUEST:
                    MasterCore.DeviceEntry host = _master.OnlineHosts.Where(x => x.AssignedID == cmd.Tag).SingleOrDefault();
                    if (host != null)
                    {
                        Console.WriteLine("HOST: Client is {0}|{1}|{2}", channel.EndPoint, channel.Id, channel.Name);
                        Console.WriteLine("CLIENT: Host is {0}|{1}|{2}", host.Channel.EndPoint, host.Channel.Id, host.Channel.Name);
                        Connection.SendCommand(host.Channel, cmd.Tag, new Commands.INTRODUCER() { RemoteEndPointAddress = channel.EndPoint.ToString() }, null);
                        Connection.SendCommand(channel, host.AssignedID, new Commands.INTRODUCER() { RemoteEndPointAddress = host.Channel.EndPoint.ToString() }, null);
                    }
                    break;
            }
        }

        public void Connected(RUDPChannel channel) { }

        void ICommandService.Connection(RUDPChannel channel)
        {
            throw new NotImplementedException();
        }
    }
}
