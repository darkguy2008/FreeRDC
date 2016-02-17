using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FreeRDC.Common.Network;

namespace FreeRDC.Services.Host.Remote
{
    public static class RDCRemoteMouse
    {
        // http://stackoverflow.com/questions/2416748/how-to-simulate-mouse-click-in-c

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private static void MouseEvent(MouseEventFlags value, Point p)
        {
            mouse_event((int)value, p.X, p.Y, 0, 0);
        }





        public static void Move(RDCMouseStruct data)
        {
            SetCursorPos(data.Position.X, data.Position.Y);
        }

        public static void Click(RDCMouseStruct data)
        {
            Move(data);
            MouseEvent(MouseEventFlags.LeftDown | MouseEventFlags.LeftUp, data.Position);
        }

        public static void Down(RDCMouseStruct data)
        {
            Move(data);
            MouseEventFlags flags = 0;
            if (data.Buttons.HasFlag(MouseButtons.Left)) { flags |= MouseEventFlags.LeftDown; }
            if (data.Buttons.HasFlag(MouseButtons.Middle)) { flags |= MouseEventFlags.MiddleDown; }
            if (data.Buttons.HasFlag(MouseButtons.Right)) { flags |= MouseEventFlags.RightDown; }
            MouseEvent(flags, data.Position);
        }

        public static void Up(RDCMouseStruct data)
        {
            Move(data);
            MouseEventFlags flags = 0;
            if (data.Buttons.HasFlag(MouseButtons.Left)) { flags |= MouseEventFlags.LeftUp; }
            if (data.Buttons.HasFlag(MouseButtons.Middle)) { flags |= MouseEventFlags.MiddleUp; }
            if (data.Buttons.HasFlag(MouseButtons.Right)) { flags |= MouseEventFlags.RightUp; }
            MouseEvent(flags, data.Position);
        }

    }
}
