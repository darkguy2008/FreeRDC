using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeRDC.Common.Network
{
    public enum RDCCommandType
    {
        ACK_STREAM_START = 1,
        ACK_STREAM_STOP = 2,

        STREAM_START = 101,
        STREAM_STOP = 102,

        MOUSE_MOVE = 200,
        MOUSE_DOWN = 210,
        MOUSE_UP = 211,

        AUTH_REQUEST_ID = 300,
        AUTH_ASSIGNED_ID = 301,

        REFRESH_FULL = 1000
    }
}
