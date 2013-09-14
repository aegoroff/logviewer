using System;

namespace logviewer.rtf.Rtf.Attributes
{
    /// <summary>
    /// Specifies control word which is added to RTF document after RtfWriter reflects all the members of a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class RtfControlWordDenotingEndAttribute : Attribute
    {
        private string _name = String.Empty;

        /// <summary>
        /// Gets control word name.
        /// </summary>
        internal string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfControlWordDenotingEndAttribute class.
        /// </summary>
        /// <param name="name">Control word name.</param>
        internal RtfControlWordDenotingEndAttribute(string name)
        {
            _name = name;
        }
    }
}