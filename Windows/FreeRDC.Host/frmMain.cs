using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FreeRDC.Services.Host;
using FreeRDC.Common.IO;
using System.Diagnostics;

namespace FreeRDC.Host
{
    public partial class frmMain : Form
    {
        public HostService Host = new HostService();
        public Dictionary<string, Dictionary<string, string>> Config;
        public string ConfigFilename { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            trayIcon.Icon = this.Icon;
            ConfigFilename = Program.AppPath + "Config.ini";
            Config = INIFile.Read(ConfigFilename);

            Host.OnConnected += new HostService.StringDataEventDelegate(Host_OnConnected);

            Host.Password = Config["FreeRDC"]["Password"];
            Host.Start(Config["FreeRDC"]["Master"]);
        }

        private void Host_OnConnected(string arg)
        {
            Invoke(new Action(() => {
                label5.Text = arg;
                label4.Text = Host.Password;
            }));
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            Process p = Process.Start(Program.AppPath + "Config.ini");
            p.WaitForExit();
            MessageBox.Show("Restart the app to apply changes");
        }

        private void shutdownFreeRDCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Host.Stop();
            Application.Exit();
        }

    }
}
