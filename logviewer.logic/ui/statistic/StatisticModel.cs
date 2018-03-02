// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 12.08.2016
// © 2012-2018 Alexander Egorov

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using logviewer.engine;
using logviewer.logic.Properties;
using logviewer.logic.storage;
using logviewer.logic.support;

namespace logviewer.logic.ui.statistic
{
    public class StatisticModel : UiSynchronizeModel
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
            var source = Observable.Create<StatItemViewModel>(observer => this.LoadFunction(observer));

            void OnNext(StatItemViewModel model) => this.items.Add(model);

            void OnError(Exception exception) => Log.Instance.Error(exception.Message, exception);

            void OnCompleted() => Log.Instance.TraceFormatted("Statistic loading completed");

            source.SubscribeOn(Scheduler.Default)
                  .ObserveOn(this.UiContextScheduler)
                  .Subscribe(OnNext, OnError, OnCompleted);
        }

        private IDisposable LoadFunction(IObserver<StatItemViewModel> observer)
        {
            this.CreateByLevelRows(observer);

            this.CreateLogPeriodRows(observer);

            CreateEmptyRow(observer);

            this.CreateTotalRow(observer);

            this.CreateSizeRow(observer);

            this.CreateLogEncodingRow(observer);

            this.CreateDatabaseFileInfoRow(observer);

            observer.OnCompleted();
            return Disposable.Empty;
        }

        private void CreateByLevelRows(IObserver<StatItemViewModel> observer)
        {
            foreach (var item in this.store.CountByLevel(this.filterViewModel.Filter, this.filterViewModel.UserRegexp, true))
            {
                observer.OnNext(CreateItem(this.levelDescriptions[(int)item.Key], item.Value));
            }
        }

        private void CreateLogPeriodRows(IObserver<StatItemViewModel> observer)
        {
            var minDateTime = this.SelectDateUsingFunc("min");
            var maxDateTime = this.SelectDateUsingFunc("max");

            if (minDateTime == maxDateTime && minDateTime == DateTime.MinValue)
            {
                return;
            }

            observer.OnNext(CreateItem(Resources.MinMessageDate, minDateTime));
            observer.OnNext(CreateItem(Resources.MaxMessageDate, maxDateTime));
        }

        private static void CreateEmptyRow(IObserver<StatItemViewModel> observer)
        {
            observer.OnNext(new StatItemViewModel());
        }

        private void CreateTotalRow(IObserver<StatItemViewModel> observer)
        {
            var total = this.store.CountMessages(LogLevel.Trace, LogLevel.Fatal, this.filterViewModel.Filter,
                                                 this.filterViewModel.UserRegexp);
            observer.OnNext(CreateItem(Resources.TotalMessages, total));
        }

        private void CreateSizeRow(IObserver<StatItemViewModel> observer)
        {
            observer.OnNext(CreateItem(Resources.Size, this.size));
        }

        private void CreateLogEncodingRow(IObserver<StatItemViewModel> observer)
        {
            observer.OnNext(CreateItem(Resources.Encoding, this.encoding));
        }

        private void CreateDatabaseFileInfoRow(IObserver<StatItemViewModel> observer)
        {
            if (string.IsNullOrWhiteSpace(this.store.DatabasePath) || !File.Exists(this.store.DatabasePath))
            {
                return;
            }

            var fi = new FileInfo(this.store.DatabasePath);
            var dbSize = fi.Length;
            observer.OnNext(CreateItem(Resources.DatabaseSize, new FileSize(dbSize).Format()));
        }

        private DateTime SelectDateUsingFunc(string func)
        {
            return this.store.SelectDateUsingFunc(func, LogLevel.Trace, LogLevel.Fatal, this.filterViewModel.Filter,
                                                  this.filterViewModel.UserRegexp);
        }

        private static StatItemViewModel CreateItem(string key, string value)
        {
            return new StatItemViewModel { Key = key, Value = value };
        }

        private static StatItemViewModel CreateItem(string key, long value)
        {
            return CreateItem(key, value.ToString("N0", CultureInfo.CurrentCulture));
        }

        private static StatItemViewModel CreateItem(string key, DateTime value)
        {
            return CreateItem(key, value.ToString("f"));
        }
    }
}
