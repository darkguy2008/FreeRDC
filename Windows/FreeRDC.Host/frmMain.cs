using FreeRDC.Common.IO;
using FreeRDC.Network.Host;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;

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

            Program.Host = new HostService();
            Program.Host.OnInitializing += Host_OnInitializing;
            Program.Host.OnInitialized += Host_OnInitialized;
            Program.Host.OnMasterConnecting += Host_OnConnecting;
            Program.Host.OnMasterConnected += Host_OnMasterConnected;
            Program.Host.OnClientConnected += Host_OnClientConnected;

            Config = INIFile.Read("Host.ini");
            HostPassword = Config["Host"]["Password"];
            MasterHostname = Config["Master"]["Hostname"];
            MasterPort = int.Parse(Config["Master"]["Port"]);

            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;
            
            bgServices = new BackgroundWorker();
            bgServices.DoWork += BgServices_DoWork;
            bgServices.RunWorkerAsync();
        }

        private void Host_OnClientConnected()
        {
            Invoke(new Action(() => {
                Hide();
            }));
        }

        private void BgServices_DoWork(object sender, DoWorkEventArgs e)
        {
            Program.Host.Init();
            Program.Host.Connect(MasterHostname, MasterPort);
        }

        private void Host_OnInitializing()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Starting up...";
            }));
        }

        private void Host_OnInitialized()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Initialized";
            }));
        }

        private void Host_OnConnecting()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Connecting...";
            }));
        }

        private void Host_OnMasterConnected(string hostId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Ready";
                txID.Text = hostId;
                txPassword.Text = Program.Host.Password;
            }));
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
    }
}
