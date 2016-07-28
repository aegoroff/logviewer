using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using logviewer.engine;
using logviewer.logic.Annotations;
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
        private string totalBytes;
        private int percent;
        private string updateAvailableText;

        private string target;
        private readonly string targetAddress;

        public UpdateViewModel(string targetAddress)
        {
            this.targetAddress = targetAddress;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool UpdateEnabled
        {
            get { return this.updateEnabled; }
            set
            {
                this.updateEnabled = value;
                this.OnPropertyChanged(nameof(this.UpdateEnabled));
            }
        }

        public bool YesEnabled
        {
            get { return this.yesEnabled; }
            set
            {
                this.yesEnabled = value;
                this.OnPropertyChanged(nameof(this.YesEnabled));
            }
        }

        public string ReadBytes
        {
            get { return this.readBytes; }
            set
            {
                this.readBytes = value;
                this.OnPropertyChanged(nameof(this.ReadBytes));
            }
        }

        public string TotalBytes
        {
            get { return this.totalBytes; }
            set
            {
                this.totalBytes = value;
                this.OnPropertyChanged(nameof(this.TotalBytes));
            }
        }

        public int Percent
        {
            get { return this.percent; }
            set
            {
                this.percent = value;
                this.OnPropertyChanged(nameof(this.Percent));
            }
        }

        public string UpdateAvailableText
        {
            get { return this.updateAvailableText; }
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
                //this.view.ShowErrorMessage(e.Message);
            }
        }

        public void StartUpdate()
        {
            try
            {
                Process.Start(this.target);
                //this.view.Close();
            }
            catch (Exception e)
            {
                //this.view.ShowErrorMessage(e.Message);
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
            this.ReadBytes = bytesIn.Format();
            this.TotalBytes = total.Format();
            this.Percent = percentage;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}