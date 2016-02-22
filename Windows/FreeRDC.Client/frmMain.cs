using System.Windows.Forms;
using System.Collections.Generic;
using FreeRDC.Services.Client;
using FreeRDC.Common.IO;

namespace FreeRDC.Client
{
    public partial class frmMain : Form
    {
        Dictionary<string, Dictionary<string, string>> Config;
        public string ConfigFilename { get; set; }
        public ClientService Client { get; set; }

        public frmMain()
        {
            InitializeComponent();
            textBox1.Text = "20740E1C";
            textBox2.Text = "1234";
        }

        private void btnConnect_Click(object sender, System.EventArgs e)
        {
            Client.OpenConnection(textBox1.Text, textBox2.Text);
        }

        private void frmMain_Load(object sender, System.EventArgs e)
        {
            Application.DoEvents();
            ConfigFilename = Program.AppPath + "Config.ini";
            Config = INIFile.Read(ConfigFilename);

            Client = new ClientService();
            Client.Start(Config["FreeRDC"]["Master"]);
        }
    }
}
