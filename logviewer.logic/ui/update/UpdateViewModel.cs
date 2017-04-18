// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 28.07.2016
// © 2012-2017 Alexander Egorov

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using logviewer.engine;
using logviewer.logic.Annotations;
using logviewer.logic.Properties;
using logviewer.logic.storage;
using logviewer.logic.support;

namespace logviewer.logic.ui.update
{
    [PublicAPI]
    public sealed class UpdateViewModel : INotifyPropertyChanged
    {
        private bool updateEnabled;

        private bool yesEnabled;

        private string readBytes;

        private int percent;

        private string updateAvailableText;

        private string target;

        private string targetAddress;

        public UpdateViewModel(Version current, Version latest, string targetAddress)
        {
            this.targetAddress = targetAddress;
            this.UpdateAvailableText = string.Format(Thread.CurrentThread.CurrentCulture, Resources.NewVersionAvailable, current, latest);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Close;

        public bool UpdateEnabled
        {
            get => this.updateEnabled;
            set
            {
                this.updateEnabled = value;
                this.OnPropertyChanged(nameof(this.UpdateEnabled));
            }
        }

        public bool YesEnabled
        {
            get => this.yesEnabled;
            set
            {
                this.yesEnabled = value;
                this.OnPropertyChanged(nameof(this.YesEnabled));
            }
        }

        public string ReadBytes
        {
            get => this.readBytes;
            set
            {
                this.readBytes = value;
                this.OnPropertyChanged(nameof(this.ReadBytes));
            }
        }

        public int Percent
        {
            get => this.percent;
            set
            {
                this.percent = value;
                this.OnPropertyChanged(nameof(this.Percent));
            }
        }

        public string UpdateAvailableText
        {
            get => this.updateAvailableText;
            set
            {
                this.updateAvailableText = value;
                this.OnPropertyChanged(nameof(this.UpdateAvailableText));
            }
        }

        public void StartDownload()
        {
            this.YesEnabled = false;

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
                this.UpdateAvailableText = e.Message;
            }
        }

        public void StartUpdate()
        {
            try
            {
                Process.Start(this.target);
                this.Close?.Invoke(this, new EventArgs());
            }
            catch (Exception e)
            {
                this.UpdateAvailableText = e.Message;
            }
        }

        private void OnComplete(EventPattern<object> eventPattern)
        {
            this.UpdateEnabled = true;
        }

        private void OnProgress(EventPattern<DownloadProgressChangedEventArgs> eventPattern)
        {
            var e = eventPattern.EventArgs;
            var bytesIn = new FileSize(e.BytesReceived, true);
            var total = new FileSize(e.TotalBytesToReceive, true);
            var percentage = bytesIn.PercentOf(total);
            this.Percent = percentage;
            this.ReadBytes = string.Format(Resources.UpdateDownloadProgressFormat, bytesIn.Format(), total.Format(), percentage);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
