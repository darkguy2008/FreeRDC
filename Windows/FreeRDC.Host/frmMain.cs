using FreeRDC.Common.IO;
using FreeRDC.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace FreeRDC.Host
{
    public partial class frmMain : Form
    {
        public bool IsExiting { get; set; }
        private BackgroundWorker bgServices;
        private Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();

        public string HostPassword
        {
            get { return Program.Host.Password; }
            set { Program.Host.Password = value; }
        }
        public string MasterHostname { get; set; }
        public int MasterPort { get; set; }

        public frmMain()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.appIcon;

            Program.Host = new HostService();
            Program.Host.OnMasterLoggedIn += Host_OnMasterLoggedIn;

            Config = INIFile.Read("Host.ini");
            HostPassword = Config["Host"]["Password"];
            MasterHostname = Config["Master"]["Hostname"];
            MasterPort = int.Parse(Config["Master"]["Port"]);

            trayIcon.Icon = Icon;
            trayIcon.ContextMenuStrip = trayMenu;

            bgServices = new BackgroundWorker() { WorkerSupportsCancellation = true };
            bgServices.DoWork += BgServices_DoWork;
        }

        private void Host_OnMasterLoggedIn(string assignedId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Ready";
                txID.Text = assignedId;
                txPassword.Text = Program.Host.Password;
                trayIcon.Text = "FreeRDC Host - " + assignedId;
            }));
        }

        private void BgServices_DoWork(object sender, DoWorkEventArgs e)
        {
            Program.Host.ConnectToMaster(MasterHostname, MasterPort);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsExiting)
                return;

            if (Config["Host"]["TipRunning"] == "1")
            {
                trayIcon.ShowBalloonTip(5000, "FreeRDC Host", "FreeRDC is still running. If you want to shut it down do it by using the right-click menu.", ToolTipIcon.Info);
                Config["Host"]["TipRunning"] = "0";
            }

            Hide();
            e.Cancel = true;
        }

        private void showHideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Visible)
                Hide();
            else
                Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("FreeRDC Host © 2016 DARKGuy");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsExiting = true;
            Program.Host.Shutdown();
            Application.Exit();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            bgServices.RunWorkerAsync();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Visible)
                Hide();
            else
            {
                Show();
                Focus();
            }
        }

        private void shutdownHostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsExiting = true;
            Program.Host.Shutdown();
            Application.Exit();
        }
    }
}
