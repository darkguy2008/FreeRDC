using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FreeRDC.Common.Crypto
{
    public static class SHA1
    {

        public static string Hash(string value)
        {
            return Convert.ToBase64String(new SHA1Managed().ComputeHash(Encoding.ASCII.GetBytes(value)));
        }

    }
}
