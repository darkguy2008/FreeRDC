using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Master
{
    public class GenericConnection : NetworkConnection
    {
        public MasterService Master { get; set; }

        public override void ProcessCommand(RDCCommandType type, object[] data)
        {
            base.ProcessCommand(type, data);
            switch (type)
            {
                case RDCCommandType.IAMHOST:
                    Master.Connections.NewHostConnection(this, (string)data[0]);
                    break;

                case RDCCommandType.IAMCLIENT:
                    Master.Connections.NewClientConnection(this);
                    break;
            }
        }

    }
}
