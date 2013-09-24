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
        private readonly Settings settings;
        private readonly ParsingTemplate template;

        public SettingsDlg(Settings settings)
        {
            this.settings = settings;
            this.InitializeComponent();
            this.openLastFile.Checked = settings.OpenLastFile;
            this.pageSizeBox.Text = settings.PageSize.ToString(CultureInfo.CurrentUICulture);
            this.keepLastNFilesBox.Text = settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

            this.template = settings.ReadParsingTemplate();
            this.messageStartPatternBox.Text = this.Template.StartMessage;

            this.traceBox.Text = this.Template.Trace;
            this.debugBox.Text = this.Template.Debug;
            this.infoBox.Text = this.Template.Info;
            this.warnBox.Text = this.Template.Warn;
            this.errorBox.Text = this.Template.Error;
            this.fatalBox.Text = this.Template.Fatal;
        }

        public ParsingTemplate Template
        {
            get { return this.template; }
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            this.settings.OpenLastFile = this.openLastFile.Checked;
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            int pageSize;
            if (int.TryParse(this.pageSizeBox.Text, out pageSize))
            {
                this.settings.PageSize = pageSize;
            }
        }

        private void OnKeyPressInNumberOnlyBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            this.template.StartMessage = this.messageStartPatternBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnSetTraceLevel(object sender, EventArgs e)
        {
            this.template.Trace = this.traceBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnSetDebugLevel(object sender, EventArgs e)
        {
            this.template.Debug = this.debugBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnSetInfoLevel(object sender, EventArgs e)
        {
            this.template.Info = this.infoBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnSetWarnLevel(object sender, EventArgs e)
        {
            this.template.Warn = this.warnBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnSetErrorLevel(object sender, EventArgs e)
        {
            this.template.Error = this.errorBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnSetFatalLevel(object sender, EventArgs e)
        {
            this.template.Fatal = this.fatalBox.Text;
            this.settings.UpdateDefaultParsingProfile(this.template);
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(this.keepLastNFilesBox.Text, out value))
            {
                this.settings.KeepLastNFiles = value;
            }
        }
    }
}