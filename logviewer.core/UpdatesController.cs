﻿// Created by: egr
// Created at: 04.10.2014
// © 2012-2015 Alexander Egorov


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using logviewer.engine;

namespace logviewer.core
{
    public class UpdatesController : BaseGuiController
    {
        private readonly string targetAddress;
        private readonly IUpdateView view;
        private string target;

        public UpdatesController(IUpdateView view, string targetAddress)
        {
            this.view = view;
            this.targetAddress = targetAddress;
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
                var source = new Uri(this.targetAddress);
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
            var bytesIn = new FileSize(e.BytesReceived, true);
            var totalBytes = new FileSize(e.TotalBytesToReceive, true);
            var percentage = bytesIn.PercentOf(totalBytes);
            this.RunOnGuiThread(
                () =>
                    this.view.OnProgress(percentage, totalBytes, bytesIn));
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