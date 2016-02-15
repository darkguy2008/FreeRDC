using System;

namespace FreeViewer.Services.Common
{
    [Serializable]
    public struct CommandStruct
    {
        public string Command;
        public object Payload;
    }
}
