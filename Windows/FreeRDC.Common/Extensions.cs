using System;
using System.Net;

namespace FreeRDC.Common
{
    public static class Extensions
    {
        public static IPEndPoint ToEndPoint(this string ipEndPoint)
        {
            int ipAddressLength = ipEndPoint.LastIndexOf(':');
            return new IPEndPoint(
                IPAddress.Parse(ipEndPoint.Substring(0, ipAddressLength)),
                Convert.ToInt32(ipEndPoint.Substring(ipAddressLength + 1)));
        }
    }
}
