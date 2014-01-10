// Created by: egr
// Created at: 10.01.2014
// © 2012-2014 Alexander Egorov


using System;

namespace logviewer.core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LogLevelAttribute : Attribute
    {
        public LogLevelAttribute(LogLevel level)
        {
            this.Level = level;
        }
        public LogLevel Level { get; private set; }
    }
}