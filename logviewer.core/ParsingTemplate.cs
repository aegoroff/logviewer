// Created by: egr
// Created at: 24.09.2013
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    public class ParsingTemplate
    {
        public int Index { get; set; }

        [Column("StartMessage")]
        public string StartMessage { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Filter", Nullable = true)]
        public string Filter { get; set; }
        
        [Column("Compiled")]
        public bool Compiled { get; set; }

        public bool IsEmpty
        {
            get { return this.StartMessage == null; }
        }
    }
}