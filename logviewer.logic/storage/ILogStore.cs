// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 14.08.2016
// © 2007-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using logviewer.engine;
using logviewer.logic.Annotations;

namespace logviewer.logic.storage
{
    [PublicAPI]
    public interface ILogStore : IDisposable
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

        void ReadMessages(
            int limit,
            Action<LogMessage> onReadMessage,
            Func<bool> notCancelled,
            DateTime start,
            DateTime finish,
            long offset = 0,
            bool reverse = true,
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true);

        string DatabasePath { get; }

        bool HasLogLevelProperty { get; }

        string LogLevelProperty { get; }
    }
}
