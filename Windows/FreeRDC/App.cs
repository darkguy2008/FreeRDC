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
        public MasterService Master { get; set; }
        public HostService Host { get; set; }

        private Thread _thConnect;

        public void Init()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm = new frmMain();
            ShuttingDown = false;

            MainForm.Show();

            _thConnect = new Thread(() =>
            {
                ReloadConfig();

                Master = new MasterService()
                {
                    Fingerprint = Fingerprint,
                    Address = Config.GetValue("FreeRDC", "Master"),
                    Port = int.Parse(Config.GetValue("FreeRDC", "Port"))
                };
                Master.OnConnected += Master_OnConnected;
                Master.OnAuthenticated += Master_OnAuthenticated;
                Master.OnIntroducerPacket += Master_OnIntroducerPacket;

                MainForm.SetStatus("Initializing...");
                Fingerprint = HWID.GenerateFingerprint();
                Master.Init();
                MainForm.SetStatus("Connecting to Master...");
                Master.Start();
            });
            _thConnect.Start();

            Application.Run();
        }

        private void Master_OnIntroducerPacket(Commands.INTRODUCER introducer)
        {
            MainForm.SetStatus("Incoming connection...");
        }

        private void Master_OnAuthenticated(string tag, string address)
        {
            AssignedTag = tag;
            OutsideAddress = address;
            MainForm.SetStatus("Ready");
            MainForm.SetTag(AssignedTag);
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
                Port = int.Parse(OutsideAddress.Split(':')[1])
            };
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
            Config.Read("FreeRDC.ini");
            HostPassword = Config.GetValue("FreeRDC", "Password");
            MainForm.RefreshUI();
        }
    }
}
