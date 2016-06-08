namespace FreeRDC.Network
{

    public enum ECommandType
    {
        AUTH = 0x10,
        AUTH_OK = 0x11,

        CLIENT_CONNECTIONREQUEST = 0x20
    }

    public static class Commands
    {
        public class AUTH
        {
            public enum AuthTypes
            {
                Host = 1,
                Client = 2
            }

            public int AuthType { get; set; }
            public string Fingerprint { get; set; }
        }

        public class AUTH_OK
        {
            public string AssignedTag { get; set; }
            public string EndpointAddress { get; set; }
        }

        public class CLIENT_CONNECTIONREQUEST
        {
            public CLIENT_CONNECTIONREQUEST()
            {
            }
        }
    }

}
