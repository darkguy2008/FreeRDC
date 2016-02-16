using System;
using System.Windows.Forms;
using FreeRDC.Services.Host;
using System.Collections.Generic;
using FreeRDC.Common.IO;

namespace FreeRDC.Host
{
    public partial class frmMain : Form
    {
        FreeRDCHost Host;
        Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            trayIcon.Icon = this.Icon;
            Host = new FreeRDCHost();

            Config = INIFile.Read(Program.AppPath + "Config.ini");
            Host.MasterServerHostname = Config["FreeRDC"]["Master"];

            Host.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Host.Stop();
            Application.Exit();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }
    }
}
