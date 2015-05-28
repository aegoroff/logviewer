// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace logviewer.engine
{
    [DebuggerDisplay("{Content}")]
    internal class Composer : List<IPattern>, IPattern
    {
        public string Content
        {
            get { return string.Join(string.Empty, this.Select(c => c.Content)); }
        }
    }
}