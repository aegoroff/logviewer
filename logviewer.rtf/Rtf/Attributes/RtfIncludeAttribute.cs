using System;

namespace logviewer.rtf.Rtf.Attributes
{
    /// <summary>
    /// Specifies wheter RtfWriter must include a member according to condition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class RtfIncludeAttribute : Attribute
    {
        private string _conditionMemberName;
        private bool _value = true;

        /// <summary>
        /// Gets or sets name of a Boolean member inside a class, value of which is used by RtfWriter as a condition. 
        /// </summary>
        public string ConditionMember
        {
            get { return _conditionMemberName; }
            set { _conditionMemberName = value; }
        }

        /// <summary>
        /// Gets or sets value of the condition member. Default is true.
        /// </summary>
        public bool Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}