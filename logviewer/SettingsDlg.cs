// Created by: egr
// Created at: 14.09.2013
// © 2012-2013 Alexander Egorov

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
            this.pageSizeBox.Text = Settings.PageSize.ToString(CultureInfo.CurrentUICulture);
            this.keepLastNFilesBox.Text = Settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);
            this.messageStartPatternBox.Text = Settings.StartMessageTemplate;
            this.traceBox.Text = Settings.TraceLevel;
            this.debugBox.Text = Settings.DebugLevel;
            this.infoBox.Text = Settings.InfoLevel;
            this.warnBox.Text = Settings.WarnLevel;
            this.errorBox.Text = Settings.ErrorLevel;
            this.fatalBox.Text = Settings.FatalLevel;
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            Settings.OpenLastFile = this.openLastFile.Checked;
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            int pageSize;
            if (int.TryParse(this.pageSizeBox.Text, out pageSize))
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
            Settings.StartMessageTemplate = this.messageStartPatternBox.Text;
        }

        private void OnSetTraceLevel(object sender, EventArgs e)
        {
            Settings.TraceLevel = this.traceBox.Text;
        }

        private void OnSetDebugLevel(object sender, EventArgs e)
        {
            Settings.DebugLevel = this.debugBox.Text;
        }

        private void OnSetInfoLevel(object sender, EventArgs e)
        {
            Settings.InfoLevel = this.infoBox.Text;
        }

        private void OnSetWarnLevel(object sender, EventArgs e)
        {
            Settings.WarnLevel = this.warnBox.Text;
        }

        private void OnSetErrorLevel(object sender, EventArgs e)
        {
            Settings.ErrorLevel = this.errorBox.Text;
        }

        private void OnSetFatalLevel(object sender, EventArgs e)
        {
            Settings.FatalLevel = this.fatalBox.Text;
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(this.keepLastNFilesBox.Text, out value))
            {
                Settings.KeepLastNFiles = value;
            }
        }
    }
}