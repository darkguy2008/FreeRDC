using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using FreeRDC.Services.Client;

namespace FreeRDC.Client
{
    public partial class frmRemote : Form
    {
        public string HostID { get; set; }
        public frmMain Main { get; internal set; }
        public HostConnection Connection { get; set; }
        private BackgroundWorker bgMonitor { get; set; }

        Point MousePos = new Point();
        public int HostScreenWidth { get; set; }
        public int HostScreenHeight { get; set; }

        public frmRemote()
        {
            InitializeComponent();
            Text = "FreeRDC - " + HostID;
            bgMonitor = new BackgroundWorker() { WorkerSupportsCancellation = true };

            //Get Current Module
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            //Assign callback function each time keyboard process
            objKeyboardProcess = new LowLevelKeyboardProc(CaptureKey);
            //Setting Hook of Keyboard Process for current module
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
        }

        public void Init()
        {
            HostScreenWidth = 0;
            HostScreenHeight = 0;
            bgMonitor.DoWork += BgMonitor_DoWork;
            bgMonitor.RunWorkerAsync();
            Connection.OnConnected += Connection_OnConnected;
            Connection.OnHostInfo += Connection_OnHostInfo;
            Connection.OnScreenRefresh += Connection_OnScreenRefresh;
            Connection.Connect();
        }

        private void Connection_OnScreenRefresh(byte[] bmpData)
        {
            MemoryStream ms = new MemoryStream(bmpData);
            Image img = Image.FromStream(ms, false, false);
            pictureBox1.Image = img;
        }

        private void Connection_OnHostInfo(int width, int height)
        {
            Invoke(new Action(() => {
                Width = width / 2;
                Height = height / 2;
            }));
        }

        private void Connection_OnConnected()
        {
        }

        private void BgMonitor_DoWork(object sender, DoWorkEventArgs e)
        {
            while(!bgMonitor.CancellationPending)
            {
                Thread.Sleep(2000);
            }
        }

        private void frmRemote_FormClosing(object sender, FormClosingEventArgs e)
        {
            bgMonitor.CancelAsync();
            Main.Show();
            Main.Focus();
            Dispose();
        }

        // http://stackoverflow.com/questions/13324059/suppress-certain-windows-keyboard-shortcuts
        // http://geekswithblogs.net/aghausman/archive/2009/04/26/disable-special-keys-in-win-app-c.aspx

        // Structure contain information about low-level keyboard input event
        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public KbdHookFlags flags;
            public int time;
            public IntPtr extra;
        }

        //System level functions to be used for hook and unhook keyboard input
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        //Declaring Global objects
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        // http://stackoverflow.com/questions/2075811/how-do-i-use-low-level-8-bit-flags-as-conditionals
        [Flags]
        enum KbdHookFlags
        {
            Extended = 0x01,
            Injected = 0x10,
            AltPressed = 0x20,
            Released = 0x80
        }

        // http://stackoverflow.com/questions/3213606/how-to-suppress-task-switch-keys-winkey-alt-tab-alt-esc-ctrl-esc-using-low
        private IntPtr CaptureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (!Focused || !tbSpecialKeys.Checked)
                return CallNextHookEx(ptrHook, nCode, wp, lp);

            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));
                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin)
                {
                    if(!objKeyInfo.flags.HasFlag(KbdHookFlags.Released))
                        Connection.HostKeyDown(new KeyEventArgs(Keys.LWin));
                    else
                        Connection.HostKeyUp(new KeyEventArgs(Keys.LWin));
                    return (IntPtr)1;
                }

                if (objKeyInfo.key == Keys.Tab)
                {
                    if (!objKeyInfo.flags.HasFlag(KbdHookFlags.Released))
                        Connection.HostKeyDown(new KeyEventArgs(Keys.Tab));
                    else
                        Connection.HostKeyUp(new KeyEventArgs(Keys.Tab));
                    return (IntPtr)1;
                }

                if (objKeyInfo.key == Keys.Escape)
                {
                    if (!objKeyInfo.flags.HasFlag(KbdHookFlags.Released))
                        Connection.HostKeyDown(new KeyEventArgs(Keys.Escape));
                    else
                        Connection.HostKeyUp(new KeyEventArgs(Keys.Escape));
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = pictureBox1.PointToClient(MousePosition);
            MousePos.X = (int)(HostScreenWidth * mouse.X / (float)pictureBox1.Width);
            MousePos.Y = (int)(HostScreenHeight * mouse.Y / (float)pictureBox1.Height);
            Connection.HostMouseMove(MousePos);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Connection.HostMouseDown(MousePos.X, MousePos.Y, e.Button);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Connection.HostMouseUp(MousePos.X, MousePos.Y, e.Button);
        }
    }
}


