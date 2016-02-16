using System;
using System.Drawing;
using System.Windows.Forms;

namespace FreeRDC.Services.Common.Commands
{
    [Serializable]
    public struct CommandMouseStruct
    {
        public int Flags;
        public Point Position;
        public MouseButtons Buttons;
    }
}
