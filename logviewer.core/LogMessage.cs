using System;
using System.Collections.Generic;
using System.Linq;

namespace logviewer.core
{
    public struct LogMessage
    {
        #region Constants and Fields

        internal IList<string> Strings;
        internal LogLevel Level;

        #endregion

        public string Header
        {
            get { return this.Strings.Count == 0 ? string.Empty : this.Strings[0]; }
        }

        public string Body
        {
            get { return this.Strings.Count < 2 ? string.Empty : string.Join(Environment.NewLine, this.MessageBody()); }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.Strings);
        }

        private IEnumerable<string> MessageBody()
        {
            var i = 0;
            return this.Strings.Where(s => i++ > 0);
        }
    }
}