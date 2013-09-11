using System;
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
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            Settings.OpenLastFile = this.openLastFile.Checked;
        }
    }
}