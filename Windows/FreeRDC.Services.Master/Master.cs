using FreeRDC.Network;
using System.Net;

namespace FreeRDC.Services
{
    public class Master
    {
        public string Fingerprint { get; set; }

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

        private void Server_OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.AUTH:
                    var cmd = _cs.DeserializeAs<Commands.AUTH>(command.Command);
                    MasterCore.HostEntry host = _master.AddHost(ep, cmd.Fingerprint);
                    _server.SendCommand(host.EndPoint, "MASTER", host.AssignedTag, new Commands.AUTH_OK() { AssignedTag = host.AssignedTag, EndpointAddress = host.EndPoint.ToString() }, null);
                    break;
            }
        }
    }
}
