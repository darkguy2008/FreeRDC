using System;
using System.Drawing;
using System.Windows.Forms;

namespace FreeViewer.Services.Common.Commands
{
    [Serializable]
    public struct CommandMouseStruct
    {
        public int Flags;
        public Point Position;
        public MouseButtons Buttons;
    }
}
