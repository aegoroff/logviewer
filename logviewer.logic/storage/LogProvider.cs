// Created by: egr
// Created at: 10.11.2015
// © 2012-2016 Alexander Egorov

using System;
using logviewer.engine;
using logviewer.logic.Annotations;
using logviewer.logic.models;
using logviewer.logic.ui;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.logic.storage
{
    public sealed class LogProvider : IItemsProvider<string>, IDisposable
    {
        private readonly ISettingsProvider settings;

        public LogProvider(LogStore store, ISettingsProvider settings)
        {
            this.Store = store;
            this.settings = settings;
        }

        [PublicAPI]
        public MessageFilterViewModel FilterViewModel { get; set; }

        [PublicAPI]
        public LogStore Store { get; set; }

        public void Dispose()
        {
            this.Store?.Dispose();
        }

        public long FetchCount()
        {
            return this.Store?.CountMessages(this.FilterViewModel.Start, this.FilterViewModel.Finish, this.FilterViewModel.Min, this.FilterViewModel.Max, this.FilterViewModel.Filter,
                this.FilterViewModel.UseRegexp, this.FilterViewModel.ExcludeNoLevel) ?? 0;
        }

        public string[] FetchRange(long offset, int limit)
        {
            var result = new string[limit];
            var ix = 0;
            this.Store?.ReadMessages(limit, message => result[ix++] = this.CreateRtf(message), () => true, this.FilterViewModel.Start,
                this.FilterViewModel.Finish, offset,
                this.FilterViewModel.Reverse,
                this.FilterViewModel.Min, this.FilterViewModel.Max, this.FilterViewModel.Filter, this.FilterViewModel.UseRegexp);
            return result;
        }

        private string CreateRtf(LogMessage message)
        {
            var doc = new RtfDocument();
            var logLvel = LogLevel.None;
            if (this.Store.HasLogLevelProperty)
            {
                logLvel = (LogLevel) message.IntegerProperty(this.Store.LogLevelProperty);
            }

            doc.AddText(message.Header.Trim(), this.settings.FormatHead(logLvel));

            var txt = message.Body;
            doc.AddNewLine();
            if (string.IsNullOrWhiteSpace(txt))
            {
                return doc.Rtf;
            }
            doc.AddText(txt.Trim(), this.settings.FormatBody(logLvel));
            doc.AddNewLine();
            return doc.Rtf;
        }
    }
}