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

        public LogProvider(LogStore store, ISettingsProvider settings)
        {
            this.Store = store;
            this.settings = settings;
        }

        public MessageFilter Filter { get; set; }

        public LogStore Store { get; set; }

        public void Dispose()
        {
            this.Store.Dispose();
        }

        public long FetchCount()
        {
            return this.Store?.CountMessages(this.Filter.Start, this.Filter.Finish, this.Filter.Min, this.Filter.Max, this.Filter.Filter,
                this.Filter.UseRegexp, this.Filter.ExcludeNoLevel) ?? 0;
        }

        public IList<string> FetchRange(long offset, int limit)
        {
            var result = new List<string>(limit);
            this.Store?.ReadMessages(limit, message => result.Add(this.CreateRtf(message)), () => true, this.Filter.Start,
                this.Filter.Finish, offset,
                this.Filter.Reverse,
                this.Filter.Min, this.Filter.Max, this.Filter.Filter, this.Filter.UseRegexp);
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