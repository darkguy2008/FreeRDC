namespace FreeRDC.Network
{

    public enum ECommandType
    {
        AUTH = 0x10,
        AUTH_OK = 0x11,

        CLIENT_CONNECTIONREQUEST = 0x20,

        INTRODUCER = 0x30,

        CLIENT_LOGIN = 0x40,
        CLIENT_LOGIN_OK = 0x41
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
            public string AssignedID { get; set; }
            public string EndpointAddress { get; set; }
        }

        public class CLIENT_CONNECTIONREQUEST { }

        public class INTRODUCER
        {
            public string RemoteEndPointAddress { get; set; }
        }

        public class CLIENT_LOGIN
        {
            public string Password { get; set; }
        }

        public class CLIENT_LOGIN_OK { }
    }

}
