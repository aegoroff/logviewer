// Created by: egr
// Created at: 04.10.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Windows.Forms;
using logviewer.core;
using logviewer.Properties;

namespace logviewer
{
    public partial class UpdateDlg : Form, IUpdateView
    {
        private readonly UpdatesController controller;

        public UpdateDlg(string message, string uri)
        {
            this.InitializeComponent();
            this.controller = new UpdatesController(this, uri);
            this.Localize(message);
        }

        public void EnableUpdateStartControl(bool enable)
        {
            this.updateBtn.Enabled = enable;
        }

        public void OnProgress(int percent, FileSize totalBytes, FileSize readBytes)
        {
            this.label2.Text = string.Format(Resources.UpdateDownloadProgressFormat, readBytes, totalBytes, percent);
            this.progressBar1.Value = percent;
        }

        public void DisableYesControl()
        {
            this.yesBtn.Enabled = false;
        }

        public void ShowErrorMessage(string message)
        {
            this.label1.Text = message;
        }

        private void Localize(string message)
        {
            this.groupBox1.Text = Resources.NewVersion;
            this.label1.Text = message;
        }

        private void OnNo(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnYes(object sender, EventArgs e)
        {
            this.controller.StartDownload();
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            this.controller.StartUpdate();
        }
    }
}