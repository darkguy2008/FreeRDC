using System.Drawing;
using System.Windows.Forms;

namespace FreeRDC.Common.UI
{
    public class UI
    {
        public static bool PasswordInput(Form parentForm, Icon icon, string title, string caption, string defaultValue, ref string userInput)
        {
            InputBox frm = new InputBox();
            frm.Icon = icon;
            frm.Text = title;
            frm.lbText.Text = caption;        
            frm.txInput.Text = defaultValue;
            frm.txInput.UseSystemPasswordChar = true;
            bool rv = frm.ShowDialog(parentForm) == DialogResult.OK;
            userInput = frm.txInput.Text;
            return rv;
        }

    }
}
