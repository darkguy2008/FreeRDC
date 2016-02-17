using System;

namespace FreeRDC.Common.Network
{
    [Serializable]
    public struct RDCCommandStruct
    {
        public RDCCommandType Command;
        public object Payload;
    }
}
