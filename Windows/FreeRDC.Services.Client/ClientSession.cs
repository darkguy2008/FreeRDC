using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Client
{
    public class ClientSession
    {
        public string SlotID { get; set; }
        public int ConnectionID { get; set; }
        public ClientService Master { get; set; }

        public ClientSession(ClientService clientService, string slotID, int cID)
        {
            Master = clientService;
            SlotID = slotID;
            ConnectionID = cID;
            Master.SendCommand(RDCCommandType.STREAM_START, SlotID, ConnectionID);
        }
    }
}
