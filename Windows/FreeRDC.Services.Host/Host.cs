using FreeRDC.Network;
using System;
using System.Net;

namespace FreeRDC.Services
{
    public class Host
    {
        private CommandConnection _master;
        private static CommandSerializer _cs = new CommandSerializer();

        public void ConnectToMaster(string address, int port)
        {
            _master = new CommandConnection();
            _master.OnConnected += OnConnected;
            _master.OnCommandReceived += OnCommandReceived;
            _master.Client(address, port);
        }

        private void OnConnected(IPEndPoint ep)
        {
            _master.RemoteEndPoint = ep;
            _master.SendCommand(_master.RemoteEndPoint, null, null, new Commands.AUTH() { AuthType = 1, Fingerprint = "LALALA" }, () =>
            {
                Console.WriteLine("Identifying...");
            });
        }

        private void OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.AUTH_OK:
                    var cmd = _cs.DeserializeAs<Commands.AUTH_OK>(command.Command);
                    if (command.TagFrom == "MASTER")
                    {
                        Console.WriteLine("Assigned tag: " + cmd.AssignedTag);
                        Console.WriteLine("Endpoint address: " + cmd.EndpointAddress);
                    }
                    break;
            }
        }
    }
}
