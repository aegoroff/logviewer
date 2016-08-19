using System;
using System.Collections.Generic;
using logviewer.engine;
using logviewer.logic.Annotations;

namespace logviewer.logic.storage
{
    [PublicAPI]
    public interface ILogStore
    {
        long CountMessages(
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false);

        long CountMessages(
            DateTime start,
            DateTime finish,
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false);

        DateTime SelectDateUsingFunc(string func, LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true);

        IEnumerable<KeyValuePair<LogLevel, long>> CountByLevel(
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false);

        IEnumerable<KeyValuePair<LogLevel, long>> CountByLevel(
            DateTime start,
            DateTime finish,
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false);

        string DatabasePath { get; }
    }
}