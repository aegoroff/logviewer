using System;

namespace logviewer.rtf.Rtf.Attributes
{
    /// <summary>
    /// Specifies control characters used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class RtfEnclosingBracesAttribute : Attribute
    {
        private bool 
            _braces = true,
            _closingSemicolon = false;

        /// <summary>
        /// Gets or sets a Boolean value indicating whether RtfWriter should enclose a class with braces.
        /// </summary>
        public bool Braces
        {
            get { return _braces; }
            set { _braces = value; }
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether RtfWriter should add a semicolon after reflecting a class.
        /// </summary>
        public bool ClosingSemicolon
        {
            get { return _closingSemicolon; }
            set { _closingSemicolon = value; }
        }
    }
}