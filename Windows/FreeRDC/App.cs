using FreeRDC.Common.Crypto;
using FreeRDC.Common.Hardware;
using FreeRDC.Common.IO;
using FreeRDC.Network;
using FreeRDC.Services.Host;
using FreeRDC.Services.Master;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System;
using FreeRDC.Common.UI;
using System.Collections.Generic;

namespace FreeRDC
{
    public class App
    {
        public frmMain MainForm { get; set; }
        public bool ShuttingDown { get; set; }
        public string Fingerprint { get; set; }
        public string HostPassword { get; set; }
        public string AssignedTag { get; set; }
        public string OutsideAddress { get; set; }

        public INIFile Config { get; set; }
        public string ConfigFilename { get; set; }
        public MasterService Master { get; set; }
        public HostService Host { get; set; }
        public List<frmRemote> Connections { get; set; }

        private Thread _thConnect;

        public void Init(string cfgFile)
        {
            ConfigFilename = cfgFile;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Connections = new List<frmRemote>();
            MainForm = new frmMain();
            ShuttingDown = false;

            MainForm.Show();

            _thConnect = new Thread(() =>
            {
                ReloadConfig();
                MainForm.SetStatus("Initializing...");
                Fingerprint = HWID.GenerateFingerprint();

                Master = new MasterService()
                {
                    Fingerprint = Fingerprint,
                    Address = Config.GetValue("FreeRDC", "Master"),
                    Port = int.Parse(Config.GetValue("FreeRDC", "Port"))
                };
                Master.OnConnected += Master_OnConnected;
                Master.OnAuthenticated += Master_OnAuthenticated;
                Master.Init();
                MainForm.SetStatus("Connecting to Master...");
                Master.Start();
            });
            _thConnect.Start();

            Application.Run();
        }

        private void Master_OnIntroducerPacket(Commands.INTRODUCER introducer)
        {
            Console.WriteLine("{0}, {1}", introducer.IncomingConnection, introducer.RemoteEndPointAddress);
            MainForm.SetStatus("Incoming connection...");
        }

        private void Master_OnAuthenticated(string tag, string address)
        {
            AssignedTag = tag;
            OutsideAddress = address;
            MainForm.SetStatus("Ready");
            MainForm.SetTag(AssignedTag);
            MainForm.ReadyToUse = true;
            MainForm.RefreshUI();
            InitHost();
        }

        private void InitHost()
        {
            if (Host != null)
                Host.Stop();
            Host = new HostService()
            {
                AssignedTag = AssignedTag,
                Address = OutsideAddress.Split(':')[0],
                Port = int.Parse(OutsideAddress.Split(':')[1]),
                Password = HostPassword
            };
            Host.Init();
            Host.Start();
        }

        public void SetPassword(string newPass)
        {
            Config.SetValue("FreeRDC", "Password", SHA1.Hash(newPass));
            Config.Save();
            ReloadConfig();
        }

        private void Master_OnConnected(IPEndPoint ep)
        {
            MainForm.SetStatus("Connected to master...");
        }

        public void Connect(string hostTag)
        {
            string pwd = string.Empty;
            if(UI.PasswordInput(MainForm, MainForm.Icon, MainForm.Text, "Please enter the host's password in order to authenticate yourself", "", ref pwd))
            {
                frmRemote frm = new frmRemote()
                {
                    HostTag = hostTag,
                    Password = pwd
                };
                Connections.Add(frm);
                frm.Init();
            }
        }

        public bool Close()
        {
            if (!ShuttingDown)
            {
                ShowHide(true);
                return false;
            }
            return true;
        }

        public void ShowHide()
        {
            ShowHide(MainForm.Visible);
        }

        public void ShowHide(bool value)
        {
            if (value)
                MainForm.Hide();
            else
                MainForm.Show();
        }

        public void ReloadConfig()
        {
            Config = new INIFile();
            Config.Read(ConfigFilename);
            HostPassword = Config.GetValue("FreeRDC", "Password");
            if(Host != null)
                Host.Password = HostPassword;
            MainForm.RefreshUI();
        }
    }
}
