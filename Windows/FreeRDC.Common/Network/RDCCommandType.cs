using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreeRDC.Common.Network
{
    public enum RDCCommandType
    {
        IAMCLIENT = 1,
        IAMHOST = 2,
        IAMHOST_OK,
        IAMCLIENT_OK,
        CLIENT_CONNECT,
        CLIENT_CONNECT_OK,
        CLIENT_CONNECT_FAIL,
        STREAM_START,
        REFRESH_FULL
    }
}
