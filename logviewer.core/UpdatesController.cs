// Created by: egr
// Created at: 04.10.2014
// © 2012-2014 Alexander Egorov


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace logviewer.core
{
    public class UpdatesController : BaseGuiController
    {
        private readonly string uri;
        private readonly IUpdateView view;
        private string target;

        public UpdatesController(IUpdateView view, string uri)
        {
            this.view = view;
            this.uri = uri;
            this.view.EnableUpdateStartControl(false);
        }

        public void StartDownload()
        {
            this.view.DisableYesControl();

            var client = new WebClient();
            client.DownloadProgressChanged += this.OnDownloadProgressChanged;
            client.DownloadFileCompleted += this.OnDownloadFileCompleted;
            try
            {
                var source = new Uri(this.uri);
                this.target = Path.Combine(SqliteSettingsProvider.ApplicationFolder, Path.GetFileName(source.LocalPath));
                client.DownloadFileAsync(source, this.target);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
                this.view.ShowErrorMessage(e.Message);
            }
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.RunOnGuiThread(
                () =>
                    this.view.EnableUpdateStartControl(true));
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var bytesIn = e.BytesReceived;
            var totalBytes = e.TotalBytesToReceive;
            var percentage = (int)((bytesIn / (double)totalBytes) * 100);
            this.RunOnGuiThread(
                () =>
                    this.view.OnProgress(percentage, new FileSize((ulong)totalBytes, true), new FileSize((ulong)bytesIn, true)));
        }

        public void StartUpdate()
        {
            try
            {
                Process.Start(this.target);
                this.view.Close();
            }
            catch (Exception e)
            {
                this.view.ShowErrorMessage(e.Message);
            }
        }
    }
}