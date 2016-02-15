using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FreeViewer.Services.Client;
using FreeViewer.Services.Common;

namespace FreeViewer.Client
{
    public partial class frmRemote : Form
    {
        FreeViewerClient client;
        Point MousePos = new Point();

        public frmRemote()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new FreeViewerClient();
            client.OnScreenUpdateFull += new FreeViewerClient.CommandDataEventDelegate(client_OnScreenUpdateFull);
            client.Start();
        }

        private void client_OnScreenUpdateFull(CommandStruct data)
        {
            MemoryStream ms = new MemoryStream((byte[])data.Payload);
            Image img = Image.FromStream(ms, false, false);
            pictureBox1.Image = img;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = pictureBox1.PointToClient(MousePosition);
            MousePos.X = (int)(800 * mouse.X / (float)pictureBox1.Width);
            MousePos.Y = (int)(600 * mouse.Y / (float)pictureBox1.Height);
            client.MouseMove(MousePos);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            client.MouseDown(MousePos, e.Button);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            client.MouseUp(MousePos, e.Button);
        }
    }
}
