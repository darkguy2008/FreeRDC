namespace FreeRDC.Network
{

    public enum ECommandType
    {
        AUTH = 0x10,
        AUTH_OK = 0x11,

        CLIENT_CONNECTIONREQUEST = 0x20,

        INTRODUCER = 0x30,

        CLIENT_LOGIN = 0x40,
        CLIENT_LOGIN_OK = 0x41,

        HOST_INFO = 0x50,
        HOST_SCREENREFRESH = 0x51,

        CLIENT_MOUSE_MOVE = 0x60,
        CLIENT_MOUSE_DOWN = 0x61,
        CLIENT_MOUSE_UP = 0x62,
        CLIENT_KEY_DOWN = 0x63,
        CLIENT_KEY_UP = 0x64
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

        public class HOST_INFO
        {
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
        }

        public class HOST_SCREENREFRESH
        {
            public byte[] Buffer { get; set; }
        }

        public class CLIENT_MOUSE_MOVE
        {
            public int MouseX { get; set; }
            public int MouseY { get; set; }
        }

        public class CLIENT_MOUSE_DOWN
        {
            public int MouseX { get; set; }
            public int MouseY { get; set; }
            public int Buttons { get; set; }
        }

        public class CLIENT_MOUSE_UP
        {
            public int MouseX { get; set; }
            public int MouseY { get; set; }
            public int Buttons { get; set; }
        }

        public class CLIENT_KEY_DOWN
        {
            public bool Alt { get; set; }
            public bool Control { get; set; }
            public bool Handled { get; set; }
            public short KeyCode { get; set; }
            public short KeyData { get; set; }
            public int KeyValue { get; set; }
            public short Modifiers { get; set; }
            public bool Shift { get; set; }
            public bool SuppressKeyPress { get; set; }
        }

        public class CLIENT_KEY_UP
        {
            public bool Alt { get; set; }
            public bool Control { get; set; }
            public bool Handled { get; set; }
            public short KeyCode { get; set; }
            public short KeyData { get; set; }
            public int KeyValue { get; set; }
            public short Modifiers { get; set; }
            public bool Shift { get; set; }
            public bool SuppressKeyPress { get; set; }
        }
    }

}
