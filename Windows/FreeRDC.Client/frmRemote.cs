using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FreeRDC.Services.Client;
using FreeRDC.Common.Network;

namespace FreeRDC.Client
{
    public partial class frmRemote : Form
    {
        RDCClientService client;
        Point MousePos = new Point();

        public frmRemote()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new RDCClientService();
            client.OnScreenUpdateFull += new RDCClientService.CommandDataEventDelegate(client_OnScreenUpdateFull);
            client.Start();
        }

        private void client_OnScreenUpdateFull(RDCCommandStruct data)
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
