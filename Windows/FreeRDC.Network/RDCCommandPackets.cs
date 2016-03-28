using System.Windows.Forms;

namespace FreeRDC.Network
{
    public static class RDCCommandPackets
    {

        public class IntroducerPacket
        {
            public string HostID { get; set; }
            public string Address { get; set; }
            public int Port { get; set; }
        }

        public class HostInfoPacket
        {
            public int ScreenWidth { get; set; }
            public int ScreenHeight { get; set; }
        }

        public class HostMouseEvent
        {
            public int MouseX { get; set; }
            public int MouseY { get; set; }
            public MouseButtons Buttons { get; set; }
        }

        public class HostKeyEvent
        {
            public bool Alt { get; set; }
            public bool Control { get; set; }
            public bool Handled { get; set; }
            public Keys KeyCode { get; set; }
            public Keys KeyData { get; set; }
            public int KeyValue { get; set; }
            public Keys Modifiers { get; set; }
            public bool Shift { get; set; }
            public bool SuppressKeyPress { get; set; }
        }

    }
}
