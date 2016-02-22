using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;
using System.Diagnostics;

namespace FreeRDC.Services.Master
{
    public class ClientConnection : NetworkConnection
    {
        public MasterService Master { get; set; }

        public override void ProcessCommand(RDCCommandType type, object[] data)
        {
            base.ProcessCommand(type, data);
            switch (type)
            {
                case RDCCommandType.CLIENT_CONNECT:
                    string usr = (string)data[0];
                    string pwd = (string)data[1];
                    Master.Sessions.Connect(this, usr, pwd);
                    break;

                case RDCCommandType.STREAM_START:
                    string slotID = (string)data[0];
                    int cID = (int)data[1];
                    Master.Sessions.SendToHost(RDCCommandType.STREAM_START, slotID, cID);
                    break;
            }
        }

    }
}
