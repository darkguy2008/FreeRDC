using FreeRDC.Common.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FreeRDC
{
    public partial class frmMain : Form
    {
        private bool _readyToUse;
        public bool ReadyToUse
        {
            get { return _readyToUse; }
            set
            {
                _readyToUse = value;
                Invoke(new Action(() =>
                {
                    btnConnect.Enabled = _readyToUse;
                    txConnect.Enabled = _readyToUse;
                    if(value)
                        txConnect.Focus();
                }));
            }
        }

        private Color _defaultInfoColor;

        public frmMain()
        {
            InitializeComponent();
            _defaultInfoColor = lbInfo.ForeColor;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ReadyToUse = false;
            txConnect.Focus();
            RefreshUI();
        }

        private void showHideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.app.ShowHide();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !Program.app.Close();
        }

        public void SetStatus(string status)
        {
            Invoke(new Action(() =>
            {
                tsStatus.Text = status;
            }));
        }

        public void SetTag(string tag)
        {
            Invoke(new Action(() =>
            {
                txTag.Text = tag;
            }));
        }

        public void SetInfo(string info, Color? color = null)
        {
            Invoke(new Action(() =>
            {
                lbInfo.Text = info;
                if (!color.HasValue)
                    lbInfo.ForeColor = _defaultInfoColor;
                else
                    lbInfo.ForeColor = color.Value;
            }));
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Program.app.ShowHide();
        }

        private void shutdownFreeRDCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.app.ShuttingDown = true;
            Close();
        }

        private void lnkPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string newPass = string.Empty;
            if (UI.PasswordInput(Program.app.MainForm, this.Icon, this.Text, "Please type a new password for clients to connect to this computer. Leave blank to disallow incoming connections.", "", ref newPass))
                Program.app.SetPassword(newPass);
        }

        public void RefreshUI()
        {
            SetInfo("");
            lnkPassword.Text = string.IsNullOrEmpty(Program.app.HostPassword) ? "Not set" : "Set";
            lnkPassword.LinkColor = string.IsNullOrEmpty(Program.app.HostPassword) ? Color.Red : Color.DarkSlateGray;
            if (string.IsNullOrEmpty(Program.app.HostPassword))
                SetInfo("Note: Incoming connections are not allowed without a password. Click Password to set.", Color.Red);
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            Process.Start(Program.AppPath + "FreeRDC.ini").WaitForExit();
            Program.app.ReloadConfig();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            Program.app.Connect(txConnect.Text);
        }
    }
}
