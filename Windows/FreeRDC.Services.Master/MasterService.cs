using FreeRDC.Network;
using SharpRUDP;
using System;
using System.Linq;
using System.Net;

namespace FreeRDC.Services
{
    public class MasterService
    {
        public bool IsListening { get { return _server.Connection.State == ConnectionState.LISTEN; } }

        private CommandConnection _server;
        private MasterCore _master;
        private static CommandSerializer _cs = new CommandSerializer();

        public void Start(string address, int port)
        {
            _master = new MasterCore(this);
            Listen(address, port);
        }

        private void Listen(string address, int port)
        {
            _server = new CommandConnection();
            _server.OnCommandReceived += Server_OnCommandReceived;
            _server.Server(address, port);
        }

        private void Server_OnCommandReceived(IPEndPoint senderEndPoint, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.AUTH:
                    var cmd = _cs.DeserializeAs<Commands.AUTH>(command.Command);
                    MasterCore.DeviceEntry device = _master.AddDevice(senderEndPoint, cmd.Fingerprint);
                    _server.SendCommand(device.EndPoint, "MASTER", new Commands.AUTH_OK() { AssignedID = device.AssignedID, EndpointAddress = device.EndPoint.ToString() });
                    break;
                case ECommandType.CLIENT_CONNECTIONREQUEST:
                    MasterCore.DeviceEntry host = _master.OnlineHosts.Where(x => x.AssignedID == command.ID).SingleOrDefault();
                    if(host != null)
                    {
                        Console.WriteLine("HOST: Client is {0}", senderEndPoint);
                        Console.WriteLine("CLIENT: Host is {0}", host.EndPoint);
                        _server.SendCommand(host.EndPoint, command.ID, new Commands.INTRODUCER() { RemoteEndPointAddress = senderEndPoint.ToString() }, null);
                        _server.SendCommand(senderEndPoint, host.AssignedID, new Commands.INTRODUCER() { RemoteEndPointAddress = host.EndPoint.ToString() }, null);
                    }
                    break;
            }
        }
    }
}
