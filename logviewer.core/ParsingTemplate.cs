// Created by: egr
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
        public string Trace { get; set; }
        
        [Column("Debug")]
        public string Debug { get; set; }
        
        [Column("Info")]
        public string Info { get; set; }

        [Column("Warn")]
        public string Warn { get; set; }

        [Column("Error")]
        public string Error { get; set; }

        [Column("Fatal")]
        public string Fatal { get; set; }

        [Column("StartMessage")]
        public string StartMessage { get; set; }

        public IEnumerable<string> Levels
        {
            get
            {
                yield return Trace;
                yield return Debug;
                yield return Info;
                yield return Warn;
                yield return Error;
                yield return Fatal;
            }
        }

        public bool IsEmpty
        {
            get { return this.StartMessage == null || this.Levels.Any(l => l == null); }
        }
    }
}