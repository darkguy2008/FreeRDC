using System;

namespace FreeRDC.Common.Network
{
    [Serializable]
    public struct CommandStruct
    {
        public string Command;
        public object Payload;
    }
}
