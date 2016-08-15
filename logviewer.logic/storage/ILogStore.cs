using System;
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

        string DatabasePath { get; }
    }
}