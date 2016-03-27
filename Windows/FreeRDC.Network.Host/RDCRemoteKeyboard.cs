using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FreeRDC.Network.Host
{
    // http://stackoverflow.com/questions/19978170/c-sharp-keyeventf-keyup-doesnt-work-in-specific-app
    public class RDCRemoteKeyboard
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;

        public static void Down(short vKey)
        {
            Debug.WriteLine("DOWN " + vKey);
            keybd_event((byte)vKey, 0, 0, 0);
        }

        public static void Up(short vKey)
        {
            Debug.WriteLine("DOWN " + vKey);
            keybd_event((byte)vKey, 0, KEYEVENTF_KEYUP, 0);
        }
    }
}