using System;

namespace logviewer.rtf.Rtf.Attributes
{
    /// <summary>
    /// Specifies control word.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class RtfControlWordAttribute : Attribute
    {
        private string _name = String.Empty;
        private bool _isIndexed = false;

        /// <summary>
        /// Gets control word name.
        /// </summary>
        internal string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating wheter the control word should be indexed when RtfWriter reflects an array.
        /// </summary>
        public bool IsIndexed
        {
            get { return _isIndexed; }
            set { _isIndexed = value; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfControlWordAttribute class.
        /// </summary>
        internal RtfControlWordAttribute()
        {
            
        }

        /// <param name="name">Control word name.</param>
        internal RtfControlWordAttribute(string name)
        {
            _name = name;
        }
    }
}