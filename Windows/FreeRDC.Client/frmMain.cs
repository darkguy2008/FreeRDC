using System.Windows.Forms;

namespace FreeRDC.Client
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, System.EventArgs e)
        {
            frmRemote frm = new frmRemote();
            Hide();
            frm.Show();
        }
    }
}
