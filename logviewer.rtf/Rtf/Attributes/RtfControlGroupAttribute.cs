using System;

namespace logviewer.rtf.Rtf.Attributes
{
    /// <summary>
    /// Specifies control group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class RtfControlGroupAttribute : Attribute
    {
        private string _name = String.Empty;

        /// <summary>
        /// Gets control group name.
        /// </summary>
        internal string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Initializes a new instance of ESCommon.Rtf.RtfControlGroupAttribute class.
        /// </summary>
        internal RtfControlGroupAttribute()
        {
            
        }

        /// <param name="name">Control group name</param>
        internal RtfControlGroupAttribute(string name)
        {
            _name = name;
        }
    }
}