namespace FreeRDC.Network
{

    public enum ECommandType
    {
        AUTH,
        AUTH_OK
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
        }
    }

}
