// Created by: egr
// Created at: 28.09.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using logviewer.core;
using logviewer.Properties;

namespace logviewer
{
    public partial class StatisticDlg : Form
    {
        private readonly LogStore store;
        private readonly string size;
        private readonly TaskScheduler uiContext;

        private readonly string[] levelDescriptions =
        {
            Resources.Trace,
            Resources.Debug,
            Resources.Info,
            Resources.Warn,
            Resources.Error,
            Resources.Fatal,
        };

        public StatisticDlg(LogStore store, string size)
        {
            this.InitializeComponent();
            this.store = store;
            this.size = size;
            this.uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            this.LoadStatistic();
        }

        private void LoadStatistic()
        {
            var total = 0UL;

            var byLevel = new ulong[(int)LogLevel.Fatal + 1];

            Action action = delegate
            {
                total = (ulong)this.store.CountMessages();
                for (var i = 0; i < byLevel.Length; i++)
                {
                    var level = (LogLevel)i;
                    byLevel[i] = (ulong)this.store.CountMessages(level, level);
                }
            };
            var job = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            Action<Task> updateUI = delegate
            {
                var items =
                    byLevel.Select(
                        (value, i) =>
                            new ListViewItem(new[] { this.levelDescriptions[i], value.ToString(value.FormatString(), CultureInfo.CurrentCulture) }))
                        .ToList();

                var sizeItem = new ListViewItem(new[] { Resources.Size, this.size });
                var totalItem =
                    new ListViewItem(new[] { Resources.TotalMessages, total.ToString(total.FormatString(), CultureInfo.CurrentCulture) });

                items.AddRange(new[] { new ListViewItem(), totalItem, sizeItem });

                this.listView1.Items.AddRange(items.ToArray());
            };

            job.ContinueWith(updateUI, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, this.uiContext);
        }
    }
}