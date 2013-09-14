using System;

namespace logviewer.rtf.Rtf.Attributes
{
    internal enum RtfTextDataType { Text, HyperLink, Raw }
    
    /// <summary>
    /// Specifies text data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class RtfTextDataAttribute : Attribute
    {
        private RtfTextDataType _type = RtfTextDataType.Text;
        private bool quotes = false;
        
        /// <summary>
        /// Gets or sets a Boolean value indicating whether RtfWriter should enclose text with quotes.
        /// </summary>
        public bool Quotes
        {
            get { return quotes; }
            set { quotes = value; }
        }

        /// <summary>
        /// Gets text data type.
        /// </summary>
        public RtfTextDataType TextDataType
        {
            get { return _type; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTextDataAttribute class.
        /// </summary>
        internal RtfTextDataAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfTextDataAttribute class.
        /// </summary>
        /// <param name="type">Text data type.</param>
        internal RtfTextDataAttribute(RtfTextDataType type)
        {
            _type = type;
        }
    }
}