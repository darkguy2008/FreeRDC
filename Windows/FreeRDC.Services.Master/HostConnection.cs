using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Master
{
    public class HostConnection : NetworkConnection
    {
        public MasterService Master { get; set; }

        public string SlotID { get; set; }
        public string HardwareFingerprint { get; set; }

        public override void Start()
        {
            base.Start();
            SlotID = Master.Sessions.RequestSlotID(HardwareFingerprint);
            SendCommand(RDCCommandType.IAMHOST_OK, SlotID);
        }

        public override void ProcessCommand(RDCCommandType type, object[] data)
        {
            base.ProcessCommand(type, data);
            switch (type)
            {
                case RDCCommandType.CLIENT_CONNECT_OK:
                    string slotID = (string)data[0];
                    int cID = (int)data[1];
                    Master.Sessions.ConnectOK(slotID, cID);
                    break;

                case RDCCommandType.REFRESH_FULL:
                    slotID = (string)data[0];
                    Master.Sessions.SendToAllClients(RDCCommandType.REFRESH_FULL, slotID, (byte[])data[1]);
                    break;
            }
        }
    }
}
