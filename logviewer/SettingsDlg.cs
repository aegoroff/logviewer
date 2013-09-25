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
        private readonly ISettingsProvider settings;
        private bool initialized;

        public SettingsDlg(ISettingsProvider settings)
        {
            this.settings = settings;
            this.InitializeComponent();
            this.LoadSettings();
        }

        public ParsingTemplate Template { get; private set; }

        private void LoadSettings()
        {
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();

            var data = new FormData();
            Action loader = delegate
            {
                data.OpenLastFile = this.settings.OpenLastFile;
                data.PageSize = this.settings.PageSize.ToString(CultureInfo.CurrentUICulture);
                data.KeepLastNFiles = this.settings.KeepLastNFiles.ToString(CultureInfo.CurrentUICulture);

                this.Template = this.settings.ReadParsingTemplate();
            };
            Action<Task> onComplete = delegate
            {
                this.openLastFile.Checked = data.OpenLastFile;
                this.pageSizeBox.Text = data.PageSize;
                this.keepLastNFilesBox.Text = data.KeepLastNFiles;

                this.messageStartPatternBox.Text = this.Template.StartMessage;
                this.traceBox.Text = this.Template.Trace;
                this.debugBox.Text = this.Template.Debug;
                this.infoBox.Text = this.Template.Info;
                this.warnBox.Text = this.Template.Warn;
                this.errorBox.Text = this.Template.Error;
                this.fatalBox.Text = this.Template.Fatal;
                this.initialized = true;
            };

            Task.Factory.StartNew(loader).ContinueWith(onComplete, CancellationToken.None, TaskContinuationOptions.None, uiContext);
        }

        private void RunOnInitialized(Action action)
        {
            if (!this.initialized)
            {
                return;
            }
            action();
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate { this.settings.OpenLastFile = this.openLastFile.Checked; });
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                int pageSize;
                if (int.TryParse(this.pageSizeBox.Text, out pageSize))
                {
                    this.settings.PageSize = pageSize;
                }
            });
        }

        private void OnKeyPressInNumberOnlyBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.StartMessage = this.messageStartPatternBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnSetTraceLevel(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.Trace = this.traceBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnSetDebugLevel(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.Debug = this.debugBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnSetInfoLevel(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.Info = this.infoBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnSetWarnLevel(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.Warn = this.warnBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnSetErrorLevel(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.Error = this.errorBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnSetFatalLevel(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                this.Template.Fatal = this.fatalBox.Text;
                this.settings.UpdateParsingProfile(this.Template);
            });
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            this.RunOnInitialized(delegate
            {
                int value;
                if (int.TryParse(this.keepLastNFilesBox.Text, out value))
                {
                    this.settings.KeepLastNFiles = value;
                }
            });
        }

        private class FormData
        {
            internal string KeepLastNFiles;
            internal bool OpenLastFile;
            internal string PageSize;
        }
    }
}