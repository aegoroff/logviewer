// Created by: egr
// Created at: 10.01.2014
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name, int logLevel = -1)
        {
            this.Name = name;
            this.LogLevel = logLevel;
        }

        public string Name { get; private set; }
        public int LogLevel { get; set; }
    }
}