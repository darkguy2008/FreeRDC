using System;
using System.Drawing;
using System.Windows.Forms;

namespace FreeRDC.Common.Network
{
    [Serializable]
    public struct RDCMouseStruct
    {
        public int Flags;
        public Point Position;
        public MouseButtons Buttons;
    }
}
