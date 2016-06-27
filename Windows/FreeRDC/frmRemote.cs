using FreeRDC.Services;
using FreeRDC.Services.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FreeRDC
{
    public partial class frmRemote : Form
    {
        public ClientService Client { get; set; }

        public string HostTag { get; set; }
        public string Password { get; set; }

        private frmWait _frmWait = new frmWait();        

        public frmRemote()
        {
            InitializeComponent();
        }

        public void Init()
        {
            Client = new ClientService()
            {
                Master = Program.app.Master
            };
            _frmWait.Show();

            new Thread(() =>
            {
                Client.Connect(HostTag, Password);
                Thread.Sleep(2000);
                _frmWait.Invoke(new Action(() =>
                {
                    _frmWait.Close();
                    _frmWait.Dispose();
                    Show();
                }));
            }).Start();
        }
    }
}
