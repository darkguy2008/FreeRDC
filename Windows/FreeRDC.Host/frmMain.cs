using FreeRDC.Common.Crypto;
using FreeRDC.Common.IO;
using FreeRDC.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace FreeRDC.Host
{
    public partial class frmMain : Form
    {
        public bool IsExiting { get; set; }
        private BackgroundWorker bgServices;
        private Dictionary<string, Dictionary<string, string>> Config = new Dictionary<string, Dictionary<string, string>>();

        public string HostToken { get; set; }
        public string HostTokenPassword
        {
            get { return Program.Host.PasswordToken; }
            set { Program.Host.PasswordToken = value; }
        }
        public string HostGlobalPassword
        {
            get { return Program.Host.PasswordGlobal; }
            set { Program.Host.PasswordGlobal = value; }
        }
        public string MasterHostname { get; set; }
        public int MasterPort { get; set; }

        public frmMain()
        {
            InitializeComponent();

            Icon = Properties.Resources.appIcon;

            Program.Host = new HostService();
            Program.Host.OnMasterLoggedIn += Host_OnMasterLoggedIn;

            LoadConfig();

            trayIcon.Icon = Icon;
            trayIcon.ContextMenuStrip = trayMenu;

            bgServices = new BackgroundWorker() { WorkerSupportsCancellation = true };
            bgServices.DoWork += BgServices_DoWork;
        }

        private void LoadConfig()
        {
            Config = INIFile.Read("Host.ini");
            HostToken = Config["Host"]["Token"];
            HostTokenPassword = Config["Host"]["TokenPassword"];
            GenerateHostToken();
            HostGlobalPassword = Config["Host"]["Password"];
            MasterHostname = Config["Master"]["Hostname"];
            MasterPort = int.Parse(Config["Master"]["Port"]);
            SaveConfig();
        }

        private void GenerateHostToken()
        {
            if(HostTokenPassword != SHA1.Hash(HostToken))
            {
                HostToken = new Random(DateTime.Now.Millisecond).Next(1000, 9000).ToString();
                HostTokenPassword = SHA1.Hash(HostToken);
                Config["Host"]["Token"] = HostToken;
                Config["Host"]["TokenPassword"] = HostTokenPassword;
                SaveConfig();
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            txPassword.Text = HostToken;
            lblPassword.Visible = !string.IsNullOrEmpty(Config["Host"]["Password"]);
        }

        private void SaveConfig()
        {
            INIFile.Save(Config, Program.AppPath + "Host.ini");
        }

        private void Host_OnMasterLoggedIn(string assignedId)
        {
            Invoke(new Action(() => {
                statusLabel.Text = "Ready";
                txID.Text = assignedId;
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
            SaveConfig();            

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

        private void configureHostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = Process.Start(Program.AppPath + "Host.ini");
            p.WaitForExit();
            MessageBox.Show("New config will be applied at next application start", "FreeRDC Host");
        }

        private void generateNewTokenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HostToken = "";
            GenerateHostToken();
        }
    }
}
