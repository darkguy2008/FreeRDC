using FreeRDC.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace FreeRDC.Services
{
    public class Host
    {
        private CommandConnection host;
        private CommandConnection master;
        private CommandSerializer _cs = new CommandSerializer();

        public void ConnectToMaster(string address, int port)
        {
            master = new CommandConnection();
            master.OnConnected += Master_OnConnected;
            master.OnCommandReceived += CommandReceivedFromMaster;
            master.Client(address, port);
        }

        private void Master_OnConnected(IPEndPoint ep)
        {
            master.RemoteEndPoint = ep;
            master.SendCommand(master.RemoteEndPoint, null, null, new Commands.AUTH() { AuthType = 1, Fingerprint = "LALALA" }, () =>
            {
                Console.WriteLine("Identifying...");
            });
        }

        private void CommandReceivedFromMaster(IPEndPoint ep, CommandSerializer serializer, CommandContainer cmd)
        {

        }
    }
}
