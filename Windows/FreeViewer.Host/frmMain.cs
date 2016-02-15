using System;
using System.Windows.Forms;
using FreeViewer.Services.Host;

namespace FreeViewer.Host
{
    public partial class frmMain : Form
    {
        FreeViewerHost host;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            trayIcon.Icon = this.Icon;
            host = new FreeViewerHost();
            host.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            host.Stop();
            Application.Exit();
        }
    }
}
