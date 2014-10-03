// Created by: egr
// Created at: 24.09.2013
// © 2012-2014 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.core
{
    public class ParsingTemplate
    {
        public int Index { get; set; }

        [LogLevel(LogLevel.Trace)]
        public string Trace { get; set; }

        [LogLevel(LogLevel.Debug)]
        public string Debug { get; set; }

        [LogLevel(LogLevel.Info)]
        public string Info { get; set; }

        [LogLevel(LogLevel.Warn)]
        public string Warn { get; set; }

        [LogLevel(LogLevel.Error)]
        public string Error { get; set; }

        [LogLevel(LogLevel.Fatal)]
        public string Fatal { get; set; }

        [Column("StartMessage")]
        public string StartMessage { get; set; }

        [Column("Name")]
        public string Name { get; set; }

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

        public bool IsEmpty
        {
            get { return this.StartMessage == null; }
        }
    }
}