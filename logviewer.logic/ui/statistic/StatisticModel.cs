using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using logviewer.engine;
using logviewer.logic.storage;

namespace logviewer.logic.ui.statistic
{
    public class StatisticModel
    {
        private readonly LogStore store;
        private readonly string size;
        private readonly string encoding;

        private readonly StatFilterViewModel filterViewModel;
        private readonly ObservableCollection<StatItemViewModel> items;

        private readonly string[] levelDescriptions =
        {
            Properties.Resources.Trace,
            Properties.Resources.Debug,
            Properties.Resources.Info,
            Properties.Resources.Warn,
            Properties.Resources.Error,
            Properties.Resources.Fatal
        };

        public StatisticModel(LogStore store, string size, string encoding)
        {
            this.store = store;
            this.size = size;
            this.encoding = encoding;
            this.filterViewModel = new StatFilterViewModel();
            this.items = new ObservableCollection<StatItemViewModel>();
        }

        public ObservableCollection<StatItemViewModel> Items => this.items;

        public StatFilterViewModel FilterViewModel => this.filterViewModel;

        public void LoadStatistic()
        {
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
                    observer.OnNext(CreateItem(Properties.Resources.MinMessageDate, minDateTime));
                    observer.OnNext(CreateItem(Properties.Resources.MaxMessageDate, maxDateTime));
                }

                observer.OnNext(new StatItemViewModel());

                var total = (ulong)this.store.CountMessages(LogLevel.Trace, LogLevel.Fatal, this.filterViewModel.Filter, this.filterViewModel.UserRegexp);
                observer.OnNext(CreateItem(Properties.Resources.TotalMessages, total));

                observer.OnNext(CreateItem(Properties.Resources.Size, this.size));
                observer.OnNext(CreateItem(Properties.Resources.Encoding, this.encoding));

                var fi = new FileInfo(this.store.DatabasePath);
                var dbSize = fi.Length;
                observer.OnNext(CreateItem(Properties.Resources.DatabaseSize, new FileSize(dbSize).Format()));

                return Disposable.Empty;
            });

            source.SubscribeOn(Scheduler.Default)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(model =>
                {
                    this.items.Add(model);
                });
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