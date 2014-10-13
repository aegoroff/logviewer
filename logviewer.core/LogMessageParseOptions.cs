// Created by: egr
// Created at: 13.10.2013
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    [Flags]
    public enum LogMessageParseOptions
    {
        None = 0,
        LogLevel = 1,
        DateTime = 2
    }
}