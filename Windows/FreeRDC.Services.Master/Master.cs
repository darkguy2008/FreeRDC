using FreeRDC.Network;
using System;
using System.Net;

namespace FreeRDC.Services
{
    public class Master
    {
        private MasterDB _db;
        private CommandConnection _server;
        private static CommandSerializer _cs = new CommandSerializer();

        public void Start(string address, int port)
        {
            _db = new MasterDB("Master.xml", true);
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
                    _server.SendCommand(ep, "MASTER", "OMGTAG1", new Commands.AUTH_OK() { AssignedTag = "OMGTAG1", EndpointAddress = ep.ToString() }, null);
                    _db.Data.Rows.Add(null, DateTime.Now, DateTime.Now, cmd.Fingerprint, "OMGTAG1");
                    _db.Save();
                    break;
            }
        }
    }
}
