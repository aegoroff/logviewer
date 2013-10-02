// Created by: egr
// Created at: 14.09.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Diagnostics;
using System.Windows.Forms;
using logviewer.core;

namespace logviewer
{
    public partial class SettingsDlg : Form, ISettingsView
    {
        private readonly SettingsController controller;
        private Action apply;

        public SettingsDlg(ISettingsProvider settings)
        {
            this.InitializeComponent();
            this.controller = new SettingsController(this, settings);
            this.controller.Load();
        }

        public void SetApplyAction(Action action)
        {
            this.apply = action;
        }

        public void EnableSave(bool enabled)
        {
            this.saveButton.Enabled = enabled;
        }

        public void LoadFormData(FormData formData)
        {
            this.openLastFile.Checked = formData.OpenLastFile;
            this.pageSizeBox.Text = formData.PageSize;
            this.keepLastNFilesBox.Text = formData.KeepLastNFiles;
        }

        public void LoadParsingTemplate(ParsingTemplate template)
        {
            this.messageStartPatternBox.Text = template.StartMessage;
            this.traceBox.Text = template.Trace;
            this.debugBox.Text = template.Debug;
            this.infoBox.Text = template.Info;
            this.warnBox.Text = template.Warn;
            this.errorBox.Text = template.Error;
            this.fatalBox.Text = template.Fatal;
        }

        private void OnCheckLastOpenedFileOption(object sender, EventArgs e)
        {
            this.controller.UpdateOpenLastFile(this.openLastFile.Checked);
        }

        private void OnSetPageSize(object sender, EventArgs e)
        {
            this.controller.UpdatePageSize(this.pageSizeBox.Text);
        }

        private void OnKeyPressInNumberOnlyBox(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsNumber(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void OnSetMessageStartPattern(object sender, EventArgs e)
        {
            this.controller.UpdateMessageStartPattern(this.messageStartPatternBox.Text);
        }

        private void OnSetTraceLevel(object sender, EventArgs e)
        {
            this.controller.UpdateTraceLevel(this.traceBox.Text);
        }

        private void OnSetDebugLevel(object sender, EventArgs e)
        {
            this.controller.UpdateDebugLevel(this.debugBox.Text);
        }

        private void OnSetInfoLevel(object sender, EventArgs e)
        {
            this.controller.UpdateInfoLevel(this.infoBox.Text);
        }

        private void OnSetWarnLevel(object sender, EventArgs e)
        {
            this.controller.UpdateWarnLevel(this.warnBox.Text);
        }

        private void OnSetErrorLevel(object sender, EventArgs e)
        {
            this.controller.UpdateErrorLevel(this.errorBox.Text);
        }

        private void OnSetFatalLevel(object sender, EventArgs e)
        {
            this.controller.UpdateFatalLevel(this.fatalBox.Text);
        }

        private void OnKeepLastNFilesChange(object sender, EventArgs e)
        {
            this.controller.UpdateKeepLastNFiles(this.keepLastNFilesBox.Text);
        }

        private void OnSave(object sender, EventArgs e)
        {
            this.controller.Save();
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            Debug.Assert(this.apply != null);
            this.apply();
        }
    }
}