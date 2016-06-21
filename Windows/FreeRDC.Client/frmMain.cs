using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using FreeRDC.Common.IO;
using FreeRDC.Services.Client;

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

            Program.Client.OnMasterLoggedIn += Client_OnMasterLoggedIn;
            Program.Client.OnNewConnection += Client_OnNewConnection;

            Config = INIFile.Read("Client.ini");
            MasterHostname = Config["Master"]["Hostname"];
            MasterPort = int.Parse(Config["Master"]["Port"]);

            trayIcon.Icon = this.Icon;
            trayIcon.ContextMenuStrip = trayMenu;

            bgServices = new BackgroundWorker();
            bgServices.DoWork += BgServices_DoWork;
            bgServices.RunWorkerAsync();
        }

        private void Client_OnNewConnection(HostConnection connection)
        {
            Invoke(new Action(() => {
                Hide();
                frmRemote frm = new frmRemote();
                frm.Main = this;
                frm.Connection = connection;
                frm.Init();
                frm.Show();
                statusLabel.Text = "Ready";
            }));
        }

        private void Client_OnMasterLoggedIn(string assignedId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Ready";
            }));
        }

        private void BgServices_DoWork(object sender, DoWorkEventArgs e)
        {
            Program.Client.ConnectToMaster(MasterHostname, MasterPort);
        }
        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Program.Client.RemoveConnections(txID.Text);
            Program.Client.ConnectToHost(txID.Text);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsExiting)
                return;

            if (Config["Client"]["TipRunning"] == "1")
            {
                trayIcon.ShowBalloonTip(5000, "FreeRDC Client", "FreeRDC is still running. If you want to shut it down do it by using the right-click menu.", ToolTipIcon.Info);
                Config["Client"]["TipRunning"] = "0";
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
            MessageBox.Show("FreeRDC Client © 2016 DARKGuy");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsExiting = true;
            // TODO: Program.Client.Shutdown();
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
