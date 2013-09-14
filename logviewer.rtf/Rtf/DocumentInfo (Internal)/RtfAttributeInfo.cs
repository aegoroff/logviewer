using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using logviewer.rtf.Rtf.Attributes;

namespace ESCommon.Rtf
{
    internal class RtfAttributeInfo
    {
        private MemberInfo _memberInfo;

        private RtfControlWordAttribute controlWordAttribute;
        private RtfControlGroupAttribute controlGroupAttribute;

        private RtfControlWordDenotingEndAttribute controlWordDenotingEndAttribute;
        private RtfEnclosingBracesAttribute enclosingBracesAttribute;

        private RtfIgnoreAttribute ignoreAttribute;
        private RtfIncludeAttribute includeAttribute;

        private RtfIndexAttribute indexAttribute;
        private RtfEnumAsControlWordAttribute enumAsControlWordAttribute;

        private RtfTextDataAttribute textDataAttribute;
        
        private bool hasAttributes = false;

        internal MemberInfo MemberInfo
        {
            get { return _memberInfo; }
        }

        internal RtfControlWordAttribute ControlWordAttribute
        {
            get { return controlWordAttribute; }
        }
        internal RtfControlGroupAttribute ControlGroupAttribute
        {
            get { return controlGroupAttribute; }
        }

        internal RtfControlWordDenotingEndAttribute ControlWordDenotingEndAttribute
        {
            get { return controlWordDenotingEndAttribute; }
        }
        internal RtfEnclosingBracesAttribute EnclosingBracesAttribute
        {
            get { return enclosingBracesAttribute; }
        }

        internal RtfIgnoreAttribute IgnoreAttribute
        {
            get { return ignoreAttribute; }
        }
        internal RtfIncludeAttribute IncludeAttribute
        {
            get { return includeAttribute; }
        }

        internal RtfIndexAttribute IndexAttribute
        {
            get { return indexAttribute; }
        }

        internal RtfEnumAsControlWordAttribute EnumAsControlWordAttribute
        {
            get { return enumAsControlWordAttribute; }
        }

        internal RtfTextDataAttribute TextDataAttribute
        {
            get { return textDataAttribute; }
        }

        internal bool HasAttributes
        {
            get { return hasAttributes; }
        }

        internal RtfAttributeInfo(MemberInfo memberInfo)
        {
            _memberInfo = memberInfo;

            object[] attributes = MemberInfo.GetCustomAttributes(false);

            hasAttributes = attributes.Length > 0;

            if (!hasAttributes)
                return;

            controlWordAttribute = GetAttribute(attributes, typeof(RtfControlWordAttribute)) as RtfControlWordAttribute;
            controlGroupAttribute = GetAttribute(attributes, typeof(RtfControlGroupAttribute)) as RtfControlGroupAttribute;

            controlWordDenotingEndAttribute = GetAttribute(attributes, typeof(RtfControlWordDenotingEndAttribute)) as RtfControlWordDenotingEndAttribute;
            enclosingBracesAttribute = GetAttribute(attributes, typeof(RtfEnclosingBracesAttribute)) as RtfEnclosingBracesAttribute;

            ignoreAttribute = GetAttribute(attributes, typeof(RtfIgnoreAttribute)) as RtfIgnoreAttribute;
            includeAttribute = GetAttribute(attributes, typeof(RtfIncludeAttribute)) as RtfIncludeAttribute;

            indexAttribute = GetAttribute(attributes, typeof(RtfIndexAttribute)) as RtfIndexAttribute;

            enumAsControlWordAttribute = GetAttribute(attributes, typeof(RtfEnumAsControlWordAttribute)) as RtfEnumAsControlWordAttribute;

            textDataAttribute = GetAttribute(attributes, typeof(RtfTextDataAttribute)) as RtfTextDataAttribute;

            object[] neededAttributes = new object[] {
                    controlWordAttribute,
                    controlGroupAttribute,
                    controlWordDenotingEndAttribute,
                    enclosingBracesAttribute,
                    ignoreAttribute,
                    includeAttribute,
                    indexAttribute,
                    enumAsControlWordAttribute,
                    textDataAttribute,
                };

            hasAttributes = false;

            for (int i = 0; !hasAttributes && i < neededAttributes.Length; i++)
                hasAttributes |= neededAttributes != null;
        }

        private object GetAttribute(object[] attributes, Type type)
        {
            foreach (object attribute in attributes)
                if (attribute.GetType() == type)
                    return attribute;

            return null;
        }
    }
}