// Created by: egr
// Created at: 12.08.2016
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using logviewer.engine;
using logviewer.logic.Properties;
using logviewer.logic.storage;
using logviewer.logic.support;

namespace logviewer.logic.ui.statistic
{
    public class StatisticModel : BaseGuiController
    {
        private readonly ILogStore store;
        private readonly string size;
        private readonly string encoding;

        private readonly StatFilterViewModel filterViewModel;
        private readonly ObservableCollection<StatItemViewModel> items;

        private readonly string[] levelDescriptions =
        {
            Resources.Trace,
            Resources.Debug,
            Resources.Info,
            Resources.Warn,
            Resources.Error,
            Resources.Fatal
        };

        public StatisticModel(ILogStore store, string size, string encoding)
        {
            this.store = store;
            this.size = size;
            this.encoding = encoding;
            this.filterViewModel = new StatFilterViewModel();
            this.filterViewModel.PropertyChanged += (sender, args) => this.LoadStatistic();
            this.items = new ObservableCollection<StatItemViewModel>();
        }

        public ObservableCollection<StatItemViewModel> Items => this.items;

        public StatFilterViewModel FilterViewModel => this.filterViewModel;

        public void LoadStatistic()
        {
            this.items.Clear();
            var source = Observable.Create<StatItemViewModel>(observer =>
            {
                for (var i = 0; i < (int)LogLevel.Fatal + 1; i++)
                {
                    var level = (LogLevel)i;
                    var value = (ulong)this.store.CountMessages(level, level, this.filterViewModel.Filter, this.filterViewModel.UserRegexp, true);
                    var item = CreateItem(this.levelDescriptions[i], value);

                    observer.OnNext(item);
                }
                
                var minDateTime = this.store.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, this.filterViewModel.Filter, this.filterViewModel.UserRegexp);
                var maxDateTime = this.store.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, this.filterViewModel.Filter, this.filterViewModel.UserRegexp);

                if (minDateTime != maxDateTime || minDateTime != DateTime.MinValue)
                {
                    observer.OnNext(CreateItem(Resources.MinMessageDate, minDateTime));
                    observer.OnNext(CreateItem(Resources.MaxMessageDate, maxDateTime));
                }

                observer.OnNext(new StatItemViewModel());

                var total = (ulong)this.store.CountMessages(LogLevel.Trace, LogLevel.Fatal, this.filterViewModel.Filter, this.filterViewModel.UserRegexp);
                observer.OnNext(CreateItem(Resources.TotalMessages, total));

                observer.OnNext(CreateItem(Resources.Size, this.size));
                observer.OnNext(CreateItem(Resources.Encoding, this.encoding));

                if (!string.IsNullOrWhiteSpace(this.store.DatabasePath) && File.Exists(this.store.DatabasePath))
                {
                    var fi = new FileInfo(this.store.DatabasePath);
                    var dbSize = fi.Length;
                    observer.OnNext(CreateItem(Resources.DatabaseSize, new FileSize(dbSize).Format()));
                }

                observer.OnCompleted();
                return Disposable.Empty;
            });

            source.SubscribeOn(Scheduler.Default)
                .ObserveOn(this.UiContextScheduler)
                .Subscribe(
                    model => { this.items.Add(model); },
                    exception => { Log.Instance.Error(exception.Message, exception); },
                    () => { Log.Instance.TraceFormatted("Statistic loading completed"); });
        }

        private static StatItemViewModel CreateItem(string key, string value)
        {
            return new StatItemViewModel { Key = key, Value = value };
        }

        private static StatItemViewModel CreateItem(string key, ulong value)
        {
            return CreateItem(key, value.ToString("N0", CultureInfo.CurrentCulture));
        }

        private static StatItemViewModel CreateItem(string key, DateTime value)
        {
            return CreateItem(key, value.ToString("f"));
        }
    }
}