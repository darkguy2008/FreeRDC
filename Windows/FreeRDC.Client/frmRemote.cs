using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System;
using FreeRDC.Network.Client;
using System.IO;

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
        }

        public void Init()
        {
            HostScreenWidth = 0;
            HostScreenHeight = 0;
            Connection.OnHostInfo += Connection_OnHostInfo;
            Connection.OnHostScreenRefresh += Connection_OnHostScreenRefresh;
            Connection.OnHostCommandTimeout += Connection_OnHostCommandTimeout;
            bgMonitor.DoWork += BgMonitor_DoWork;
            bgMonitor.RunWorkerAsync();
            Connection.ScreenRefresh();
        }

        private void Connection_OnHostCommandTimeout()
        {
            MessageBox.Show("Connection closed");
            Close();
        }

        private void Connection_OnHostScreenRefresh(byte[] imageData)
        {
            Connection.ScreenRefresh();
            MemoryStream ms = new MemoryStream(imageData);
            Image img = Image.FromStream(ms, false, false);
            pictureBox1.Image = img;
        }

        private void Connection_OnHostInfo(int screenWidth, int screenHeight)
        {
            HostScreenWidth = screenWidth;
            HostScreenHeight = screenHeight;
            Invoke(new Action(() => {
                Width = HostScreenWidth / 2;
                Height = HostScreenHeight / 2;
            }));
        }

        private void BgMonitor_DoWork(object sender, DoWorkEventArgs e)
        {
            while(!bgMonitor.CancellationPending)
            {
                if (HostScreenWidth == 0 || HostScreenHeight == 0)
                    Connection.GetHostInfo();
                Thread.Sleep(2000);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = pictureBox1.PointToClient(MousePosition);
            MousePos.X = (int)(HostScreenWidth * mouse.X / (float)pictureBox1.Width);
            MousePos.Y = (int)(HostScreenHeight * mouse.Y / (float)pictureBox1.Height);
            Connection.HostMouseMove(HostID, MousePos);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Connection.HostMouseDown(MousePos.X, MousePos.Y, e.Button);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Connection.HostMouseUp(MousePos.X, MousePos.Y, e.Button);
        }

        private void frmRemote_KeyDown(object sender, KeyEventArgs e)
        {
            Connection.HostKeyDown(e);
        }

        private void frmRemote_KeyUp(object sender, KeyEventArgs e)
        {
            Connection.HostKeyUp(e);
        }

        private void frmRemote_FormClosing(object sender, FormClosingEventArgs e)
        {
            Connection.Disconnect();
            bgMonitor.CancelAsync();
            Main.Show();
            Main.Focus();
            Dispose();
        }
    }
}


