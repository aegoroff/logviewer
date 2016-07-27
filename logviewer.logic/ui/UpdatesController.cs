// Created by: egr
// Created at: 04.10.2014
// © 2012-2016 Alexander Egorov

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using logviewer.engine;
using logviewer.logic.storage;
using logviewer.logic.support;

namespace logviewer.logic.ui
{
    public class UpdatesController
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

            var onProgress = Observable.FromEventPattern<DownloadProgressChangedEventArgs>(client, nameof(client.DownloadProgressChanged));
            var onComplete = Observable.FromEventPattern(client, nameof(client.DownloadFileCompleted));
            onProgress.ObserveOn(Scheduler.CurrentThread).Subscribe(this.OnProgress);
            onComplete.ObserveOn(Scheduler.CurrentThread).Subscribe(this.OnComplete);
            
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

        private void OnComplete(EventPattern<object> eventPattern)
        {
            this.view.EnableUpdateStartControl(true);
        }

        private void OnProgress(EventPattern<DownloadProgressChangedEventArgs> eventPattern)
        {
            var e = eventPattern.EventArgs;
            var bytesIn = new FileSize(e.BytesReceived, true);
            var totalBytes = new FileSize(e.TotalBytesToReceive, true);
            var percentage = bytesIn.PercentOf(totalBytes);
            this.view.OnProgress(percentage, totalBytes, bytesIn);
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