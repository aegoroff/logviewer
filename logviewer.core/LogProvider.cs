// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using logviewer.engine;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.core
{
    public sealed class LogProvider : IItemsProvider<string>, IDisposable
    {
        private readonly ISettingsProvider settings;
        private readonly LogStore store;

        public LogProvider(LogStore store, ISettingsProvider settings)
        {
            this.store = store;
            this.settings = settings;
        }

        public MessageFilter Filter { get; set; }

        public void Dispose()
        {
            this.store.Dispose();
        }

        public long FetchCount()
        {
            return this.store.CountMessages(this.Filter.Start, this.Filter.Finish, this.Filter.Min, this.Filter.Max, this.Filter.Filter,
                this.Filter.UseRegexp, this.Filter.ExcludeNoLevel);
        }

        public IList<string> FetchRange(long offset, int limit)
        {
            var result = new List<string>(limit);
            this.store.ReadMessages(limit, message => result.Add(this.AddMessage(message)), () => true, this.Filter.Start,
                this.Filter.Finish, offset,
                this.Filter.Reverse,
                this.Filter.Min, this.Filter.Max, this.Filter.Filter, this.Filter.UseRegexp);
            return result;
        }

        private string AddMessage(LogMessage message)
        {
            var doc = new RtfDocument();
            var logLvel = LogLevel.None;
            if (this.store.HasLogLevelProperty)
            {
                logLvel = (LogLevel) message.IntegerProperty(this.store.LogLevelProperty);
            }

            doc.AddText(message.Header.Trim(), this.settings.FormatHead(logLvel));

            var txt = message.Body;
            if (string.IsNullOrWhiteSpace(txt))
            {
                return doc.Rtf;
            }
            doc.AddNewLine();
            doc.AddText(txt.Trim(), this.settings.FormatBody(logLvel));
            doc.AddNewLine();
            return doc.Rtf;
        }
    }
}