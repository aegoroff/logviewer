// Created by: egr
// Created at: 28.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using logviewer.core;
using logviewer.engine;
using logviewer.Properties;

namespace logviewer
{
    public partial class StatisticDlg : Form
    {
        private readonly LogStore store;
        private readonly string size;
        private readonly string encoding;
        private readonly TaskScheduler uiContext;

        private readonly string[] levelDescriptions =
        {
            Resources.Trace,
            Resources.Debug,
            Resources.Info,
            Resources.Warn,
            Resources.Error,
            Resources.Fatal
        };

        public StatisticDlg(LogStore store, string size, string encoding)
        {
            this.InitializeComponent();
            this.store = store;
            this.size = size;
            this.encoding = encoding;
            this.uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            this.LoadStatistic();
        }

        private void LoadStatistic()
        {
            var total = 0UL;

            var byLevel = new ulong[(int)LogLevel.Fatal + 1];

            var filter = textBox1.Text;
            var regexp = this.useRegexp.Checked;
            var dbSize = 0L;
            DateTime minDateTime = DateTime.MinValue;
            DateTime maxDateTime = DateTime.MinValue;

            Action action = delegate
            {
                total = (ulong)this.store.CountMessages(LogLevel.Trace, LogLevel.Fatal, filter, regexp);
                for (var i = 0; i < byLevel.Length; i++)
                {
                    var level = (LogLevel)i;
                    byLevel[i] = (ulong)this.store.CountMessages(level, level, filter, regexp, true);
                }
                var fi = new FileInfo(this.store.DatabasePath);
                dbSize = fi.Length;
                minDateTime = this.store.SelectDateUsingFunc("min", LogLevel.Trace, LogLevel.Fatal, filter, regexp);
                maxDateTime = this.store.SelectDateUsingFunc("max", LogLevel.Trace, LogLevel.Fatal, filter, regexp);
            };
            var job = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            Action<Task> updateUi = delegate
            {
                var items =
                    byLevel.Select(
                        (value, i) =>
                            new ListViewItem(new[] { this.levelDescriptions[i], value.ToString(value.FormatString(), CultureInfo.CurrentCulture) }))
                        .ToList();

                var sizeItem = new ListViewItem(new[] { Resources.Size, this.size });
                var encodingItem = new ListViewItem(new[] { Resources.Encoding, this.encoding });
                var databaseSize = new ListViewItem(new[] { Resources.DatabaseSize, new FileSize(dbSize).Format() });
                var minDate = new ListViewItem(new[] { Resources.MinMessageDate, minDateTime.ToString("f") });
                var maxDate = new ListViewItem(new[] { Resources.MaxMessageDate, maxDateTime.ToString("f") });

                if (minDateTime != maxDateTime || minDateTime != DateTime.MinValue)
                {
                    items.Add(minDate);
                    items.Add(maxDate);
                }

                var totalItem =
                    new ListViewItem(new[] { Resources.TotalMessages, total.ToString(total.FormatString(), CultureInfo.CurrentCulture) });

                items.AddRange(new[] { new ListViewItem(), totalItem, sizeItem, encodingItem, databaseSize });

                this.listView1.Items.AddRange(items.ToArray());
            };

            job.ContinueWith(updateUi, CancellationToken.None, TaskContinuationOptions.NotOnCanceled, this.uiContext);
        }

        private void OnOk(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnFilter(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            this.LoadStatistic();
        }
    }
}