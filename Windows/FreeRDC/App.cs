using FreeRDC.Common.Crypto;
using FreeRDC.Common.IO;
using FreeRDC.Network;
using FreeRDC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace FreeRDC
{
    public class App
    {
        public FreeRDCService MainService { get; set; }

        public frmMain MainForm { get; set; }
        public bool ShuttingDown { get; set; }
        public INIFile Config { get; set; }
        public string ConfigFilename { get; set; }
        public List<frmRemote> Connections { get; set; }

        private CommandConnection _outsideConnection;
        private Thread _thConnect;

        public void Init(string cfgFile)
        {
            ConfigFilename = cfgFile;
            MainService = new FreeRDCService();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _outsideConnection = new CommandConnection();
            Connections = new List<frmRemote>();
            MainForm = new frmMain();
            ShuttingDown = false;

            MainForm.Show();

            _thConnect = new Thread(() =>
            {
                ReloadConfig();
                MainForm.SetStatus("Initializing...");
                MainService.Address = Config.GetValue("FreeRDC", "Master");
                MainService.Port = int.Parse(Config.GetValue("FreeRDC", "Port"));
                MainService.OnConnectedToMaster += Master_OnConnectedToMaster;
                MainService.OnConnectedToHost += Master_OnConnectedToHost;
                MainService.OnAuthenticatedToMaster += Master_OnAuthenticatedToMaster;
                MainService.OnIncomingClientConnection += Master_OnIncomingClientConnection;
                MainService.Connection = _outsideConnection;
                MainService.Init();
                MainForm.SetStatus("Connecting to Master...");
                MainService.Start();
            });
            _thConnect.Start();

            Application.Run();
        }

        private void Master_OnIncomingClientConnection(Commands.INTRODUCER introducer)
        {
            Console.WriteLine("{0}, {1}", introducer.IncomingConnection, introducer.RemoteEndPointAddress);
            MainForm.SetStatus("Incoming connection...");
        }

        private void Master_OnConnectedToHost(CommandContainer cmd, Commands.INTRODUCER introducer)
        {
            frmRemote frmConnection = Connections.Where(x => x.HostTag == cmd.Tag).FirstOrDefault();
            if (frmConnection != null)
            {
                frmConnection.Client.HostAddress = introducer.RemoteEndPointAddress.Split(':')[0];
                frmConnection.Client.HostPort = int.Parse(introducer.RemoteEndPointAddress.Split(':')[1]);
                frmConnection.PasswordPrompt();
            }
        }

        private void Master_OnAuthenticatedToMaster(string tag, string address)
        {
            MainForm.SetStatus("Ready");
            MainForm.SetTag(MainService.AssignedTag);
            MainForm.ReadyToUse = true;
            MainForm.RefreshUI();
        }

        public void SetPassword(string newPass)
        {
            Config.SetValue("FreeRDC", "Password", SHA1.Hash(newPass));
            Config.Save();
            ReloadConfig();
        }

        private void Master_OnConnectedToMaster()
        {
            MainForm.SetStatus("Connected to master...");
        }

        public void Connect(string hostTag)
        {
            frmRemote frm = new frmRemote()
            {
                HostTag = hostTag,
                MainForm = MainForm,
                ClientConnection = _outsideConnection
            };
            Connections.Add(frm);
            frm.Init();
            frm.Connect();
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
            MainService.HostPassword = Config.GetValue("FreeRDC", "Password");
            MainForm.RefreshUI();
        }
    }
}
