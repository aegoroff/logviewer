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
            keepLastNFilesBox.Text = Settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);
            messageStartPatternBox.Text = Settings.StartMessageTemplate;
            traceBox.Text = Settings.TraceLevel;
            debugBox.Text = Settings.DebugLevel;
            infoBox.Text = Settings.InfoLevel;
            warnBox.Text = Settings.WarnLevel;
            errorBox.Text = Settings.ErrorLevel;
            fatalBox.Text = Settings.FatalLevel;
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

        private void OnKeyPressInNumberOnlyBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            Settings.StartMessageTemplate = messageStartPatternBox.Text;
        }

        private void OnSetTraceLevel(object sender, EventArgs e)
        {
            Settings.TraceLevel = traceBox.Text;
        }

        private void OnSetDebugLevel(object sender, EventArgs e)
        {
            Settings.DebugLevel = debugBox.Text;
        }

        private void OnSetInfoLevel(object sender, EventArgs e)
        {
            Settings.InfoLevel = infoBox.Text;
        }

        private void OnSetWarnLevel(object sender, EventArgs e)
        {
            Settings.WarnLevel = warnBox.Text;
        }

        private void OnSetErrorLevel(object sender, EventArgs e)
        {
            Settings.ErrorLevel = errorBox.Text;
        }

        private void OnSetFatalLevel(object sender, EventArgs e)
        {
            Settings.FatalLevel = fatalBox.Text;
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(keepLastNFilesBox.Text, out value))
            {
                Settings.KeepLastNFiles = value;
            }
        }
    }
}