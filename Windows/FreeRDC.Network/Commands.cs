namespace FreeRDC.Network
{

    public enum ECommandType
    {
        AUTH = 0x10,
        AUTH_OK = 0x11
    }

    public static class Commands
    {
        public class AUTH
        {
            public int AuthType { get; set; }
            public string Fingerprint { get; set; }
        }

        public class AUTH_OK
        {
            public string AssignedTag { get; set; }
            public string EndpointAddress { get; set; }
        }
    }

}
