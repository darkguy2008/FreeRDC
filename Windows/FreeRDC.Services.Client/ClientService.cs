using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;
using System.Net.Sockets;

namespace FreeRDC.Services.Client
{
    public class ClientService : NetworkConnection
    {
        public string MasterHostname { get; set; }
        public List<ClientSession> Sessions = new List<ClientSession>();

        public void Start(string masterHostname)
        {
            MasterHostname = masterHostname;
            Connection = new TcpClient();
            Connection.Connect(MasterHostname, 80);
            base.Start();
            SendCommand(RDCCommandType.IAMCLIENT, null);
        }

        public override void ProcessCommand(RDCCommandType type, object[] data)
        {
            base.ProcessCommand(type, data);
            switch (type)
            {
                case RDCCommandType.IAMCLIENT_OK:
                    break;

                case RDCCommandType.CLIENT_CONNECT_OK:
                    string slotID = (string)data[0];
                    int cID = (int)data[1];
                    if (Sessions.Where(x => x.SlotID == slotID).Count() == 0)
                        Sessions.Add(new ClientSession(this, slotID, cID));
                    break;
            }
        }

        public void OpenConnection(string slotID, string password)
        {
            SendCommand(RDCCommandType.CLIENT_CONNECT, slotID, password);
        }
    }
}
