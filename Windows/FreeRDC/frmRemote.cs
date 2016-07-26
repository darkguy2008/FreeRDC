using FreeRDC.Common.UI;
using FreeRDC.Network;
using FreeRDC.Services.Client;
using System;
using System.Windows.Forms;

namespace FreeRDC
{
    public partial class frmRemote : Form
    {
        public ClientService Client { get; set; }

        public string HostTag { get; set; }
        public CommandConnection ClientConnection { get; set; }
        public frmMain MainForm { get; set; }

        private frmWait _frmWait = new frmWait();        

        public frmRemote()
        {
            InitializeComponent();
        }

        public void Init()
        {
            Client = new ClientService()
            {
                Service = Program.app.MainService,
                ClientConnection = ClientConnection
            };
            _frmWait.Show();
        }

        public void Connect()
        { 
            Client.Connect(HostTag);
        }

        public void PasswordPrompt()
        {
            _frmWait.Invoke(new Action(() =>
            {
                string pwd = string.Empty;
                if (UI.PasswordInput(MainForm, MainForm.Icon, MainForm.Text, "Please enter the host's password in order to authenticate yourself", "", ref pwd))
                {
                    Client.OnLoggedIn += Client_OnLoggedIn;
                    Client.Login(pwd);
                }
            }));
        }

        private void Client_OnLoggedIn()
        {
            Client.OnLoggedIn -= Client_OnLoggedIn;
            _frmWait.Invoke(new Action(() =>
            {
                _frmWait.Close();
                _frmWait.Dispose();
                Show();
            }));
        }
    }
}
