﻿// Created by: egr
// Created at: 24.09.2013
// © 2012-2013 Alexander Egorov

using System.Collections.Generic;
using System.Linq;

namespace logviewer.core
{
    public class ParsingTemplate
    {
        public int Index { get; set; }

        [Column("Trace")]
        [LogLevel(LogLevel.Trace)]
        public string Trace { get; set; }

        [Column("Debug")]
        [LogLevel(LogLevel.Debug)]
        public string Debug { get; set; }

        [Column("Info")]
        [LogLevel(LogLevel.Info)]
        public string Info { get; set; }

        [Column("Warn")]
        [LogLevel(LogLevel.Warn)]
        public string Warn { get; set; }

        [Column("Error")]
        [LogLevel(LogLevel.Error)]
        public string Error { get; set; }

        [Column("Fatal")]
        [LogLevel(LogLevel.Fatal)]
        public string Fatal { get; set; }

        [Column("StartMessage")]
        public string StartMessage { get; set; }

        public IEnumerable<string> Levels
        {
            get
            {
                yield return this.Trace;
                yield return this.Debug;
                yield return this.Info;
                yield return this.Warn;
                yield return this.Error;
                yield return this.Fatal;
            }
        }

        public void UpdateLevelProperty(string value, LogLevel level)
        {
            foreach (var property in from attribute in
                                         (from property in typeof(ParsingTemplate).GetProperties()
                                          where property.IsDefined(typeof(LogLevelAttribute), false)
                                          select property)
                                     let attr = (LogLevelAttribute)attribute.GetCustomAttributes(typeof(LogLevelAttribute), false)[0]
                                     where attr.Level == level
                                     select attribute)
            {
                property.SetValue(this, value, null);
            }
        }

        public bool IsEmpty
        {
            get { return this.StartMessage == null || this.Levels.Any(value => value == null); }
        }
    }
}