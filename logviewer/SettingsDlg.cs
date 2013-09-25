// Created by: egr
// Created at: 14.09.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using logviewer.core;

namespace logviewer
{
    public partial class SettingsDlg : Form
    {
        private readonly FormData formData = new FormData();
        private readonly ISettingsProvider settings;
        private bool initialized;
        private ParsingTemplate template;

        public SettingsDlg(ISettingsProvider settings)
        {
            this.settings = settings;
            this.InitializeComponent();
            this.LoadSettings();
        }

        private void LoadSettings()
        {
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            Action loader = delegate
            {
                this.formData.OpenLastFile = this.settings.OpenLastFile;
                this.formData.PageSize = this.settings.PageSize.ToString(CultureInfo.CurrentUICulture);
                this.formData.KeepLastNFiles = this.settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

                this.template = this.settings.ReadParsingTemplate();
            };
            Action<Task> onComplete = delegate
            {
                this.openLastFile.Checked = this.formData.OpenLastFile;
                this.pageSizeBox.Text = this.formData.PageSize;
                this.keepLastNFilesBox.Text = this.formData.KeepLastNFiles;

                this.messageStartPatternBox.Text = this.template.StartMessage;
                this.traceBox.Text = this.template.Trace;
                this.debugBox.Text = this.template.Debug;
                this.infoBox.Text = this.template.Info;
                this.warnBox.Text = this.template.Warn;
                this.errorBox.Text = this.template.Error;
                this.fatalBox.Text = this.template.Fatal;
                this.initialized = true;
                this.EnableSave(false);
            };

            Task.Factory.StartNew(loader).ContinueWith(onComplete, CancellationToken.None, TaskContinuationOptions.None, uiContext);
        }

        private void EnableSave(bool enabled)
        {
            this.saveButton.Enabled = enabled;
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            this.formData.OpenLastFile = this.openLastFile.Checked;
            this.EnableSave(true);
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            this.formData.PageSize = this.pageSizeBox.Text;
            this.EnableSave(true);
        }

        private void OnKeyPressInNumberOnlyBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            this.template.StartMessage = this.messageStartPatternBox.Text;
            this.EnableSave(true);
        }

        private void OnSetTraceLevel(object sender, EventArgs e)
        {
            this.template.Trace = this.traceBox.Text;
            this.EnableSave(true);
        }

        private void OnSetDebugLevel(object sender, EventArgs e)
        {
            this.template.Debug = this.debugBox.Text;
            this.EnableSave(true);
        }

        private void OnSetInfoLevel(object sender, EventArgs e)
        {
            this.template.Info = this.infoBox.Text;
            this.EnableSave(true);
        }

        private void OnSetWarnLevel(object sender, EventArgs e)
        {
            this.template.Warn = this.warnBox.Text;
            this.EnableSave(true);
        }

        private void OnSetErrorLevel(object sender, EventArgs e)
        {
            this.template.Error = this.errorBox.Text;
            this.EnableSave(true);
        }

        private void OnSetFatalLevel(object sender, EventArgs e)
        {
            this.template.Fatal = this.fatalBox.Text;
            this.EnableSave(true);
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            this.formData.KeepLastNFiles = this.keepLastNFilesBox.Text;
            this.EnableSave(true);
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (!this.initialized)
            {
                return;
            }
            int pageSize;
            if (int.TryParse(this.formData.PageSize, out pageSize))
            {
                this.settings.PageSize = pageSize;
            }
            int value;
            if (int.TryParse(this.formData.KeepLastNFiles, out value))
            {
                this.settings.KeepLastNFiles = value;
            }
            this.settings.UpdateParsingProfile(this.template);
            this.EnableSave(false);
        }

        private class FormData
        {
            internal string KeepLastNFiles;
            internal bool OpenLastFile;
            internal string PageSize;
        }
    }
}