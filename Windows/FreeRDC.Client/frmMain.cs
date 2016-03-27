using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using FreeRDC.Common.IO;

namespace FreeRDC.Client
{
    public partial class frmMain : Form
    {
        public bool IsExiting { get; set; }
        private BackgroundWorker bgServices;
        private Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();

        public string MasterHostname { get; set; }
        public int MasterPort { get; set; }

        public frmMain()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.appIcon;

            Program.Client.OnInitializing += Client_OnInitializing;
            Program.Client.OnInitialized += Client_OnInitialized;
            Program.Client.OnConnectingToMaster += Client_OnConnectingToMaster;
            Program.Client.OnConnectedToMaster += Client_OnConnectedToMaster;
            Program.Client.OnHostConnectionError += Client_OnHostConnectionError;
            Program.Client.OnHostConnecting += Client_OnHostConnecting;
            Program.Client.OnHostConnected += Client_OnHostConnected;
            Program.Client.OnHostError += Client_OnHostError;

            Config = INIFile.Read("Host.ini");
            MasterHostname = Config["Master"]["Hostname"];
            MasterPort = int.Parse(Config["Master"]["Port"]);

            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;

            bgServices = new BackgroundWorker();
            bgServices.DoWork += BgServices_DoWork;
            bgServices.RunWorkerAsync();
        }

        private void Client_OnHostConnectionError(string hostId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = hostId + " not online.";
            }));
        }

        private void Client_OnHostError(string hostId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = hostId + " error.";
            }));
        }

        private void Client_OnHostConnecting(string hostId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Connecting to " + hostId + "...";
            }));
        }

        private void Client_OnHostConnected(string hostId)
        {
            Invoke(new Action(() => {
                Hide();
                frmRemote frm = new frmRemote();
                frm.Main = this;
                frm.HostID = hostId;
                frm.Connection = Program.Client.HostConnections.Where(x => x.HostID == hostId).Single();
                frm.Init();
                frm.Show();
                statusLabel.Text = "Ready";
            }));
        }

        private void BgServices_DoWork(object sender, DoWorkEventArgs e)
        {
            Program.Client.Init();
            Program.Client.Connect(MasterHostname, MasterPort);
        }

        private void Client_OnInitializing()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Starting up...";
            }));
        }

        private void Client_OnInitialized()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Initialized";
            }));
        }

        private void Client_OnConnectingToMaster()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Connecting...";
            }));
        }

        private void Client_OnConnectedToMaster()
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Ready";
            }));
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Program.Client.RemoveConnections(txID.Text);
            Program.Client.ConnectHost(txID.Text, txPassword.Text);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            MessageBox.Show("FreeRDC Client © 2016 DARKGuy");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsExiting = true;
            Program.Client.Shutdown();
            Application.Exit();
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
    }
}
