using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace ESCommon.Rtf
{
    internal class RtfTypeInfo
    {
        private Type _type;

        private MemberInfo[] members;

        internal Type Type
        {
            get { return _type; }
        }

        internal MemberInfo[] Members
        {
            get { return members; }
        }

        internal RtfTypeInfo(Type type)
        {
            _type = type;
            members = _type.FindMembers(MemberTypes.Field | MemberTypes.Property, BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance, FilterHasAttributes, null);
        }

        private bool FilterHasAttributes(MemberInfo m, object filterCriteria)
        {
            RtfAttributeInfo info = RtfDocumentInfo.GetAttributeInfo(m);
            return info.HasAttributes;
        }
    }
}