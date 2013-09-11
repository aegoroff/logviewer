using System;
using System.Globalization;
using System.Windows.Forms;
using logviewer.core;

namespace logviewer
{
    public partial class SettingsDlg : Form
    {
        public SettingsDlg()
        {
            this.InitializeComponent();
            this.openLastFile.Checked = Settings.OpenLastFile;
            pageSizeBox.Text = Settings.PageSize.ToString(CultureInfo.CurrentUICulture);
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            Settings.OpenLastFile = this.openLastFile.Checked;
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            int pageSize;
            if (int.TryParse(pageSizeBox.Text, out pageSize))
            {
                Settings.PageSize = pageSize;
            }
        }

        private void OnKeyPressInPageSizeBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
    }
}