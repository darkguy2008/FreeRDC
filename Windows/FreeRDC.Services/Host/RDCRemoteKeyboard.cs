using System.Runtime.InteropServices;

namespace FreeRDC.Services.Host
{
    // http://stackoverflow.com/questions/19978170/c-sharp-keyeventf-keyup-doesnt-work-in-specific-app
    public class RDCRemoteKeyboard
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        public static void Down(short vKey, bool shift)
        {
            if (vKey == 0x10) // shift key
                keybd_event((byte)vKey, 0, 0, 0);
            else
                keybd_event((byte)vKey, 0, shift ? KEYEVENTF_EXTENDEDKEY : 0, 0);
        }

        public static void Up(short vKey, bool shift)
        {
            if (vKey == 0x10) // shift key
                keybd_event((byte)vKey, 0, KEYEVENTF_KEYUP, 0);
            else
                keybd_event((byte)vKey, 0, KEYEVENTF_KEYUP | (shift ? KEYEVENTF_EXTENDEDKEY : 0), 0);
        }
    }
}