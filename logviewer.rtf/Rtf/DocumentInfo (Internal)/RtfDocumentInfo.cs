using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.IO;

namespace ESCommon.Rtf
{
    internal static class RtfDocumentInfo
    {
        private static ArrayList 
            attributeInfoItems = new ArrayList(),
            typeInfoItems = new ArrayList();

        internal static RtfAttributeInfo GetAttributeInfo(MemberInfo memberInfo)
        {
            foreach (RtfAttributeInfo item in attributeInfoItems)
                if (item.MemberInfo == memberInfo)
                    return item;

            RtfAttributeInfo info = new RtfAttributeInfo(memberInfo);
            attributeInfoItems.Add(info);

            return info;
        }

        internal static MemberInfo[] GetTypeMembers(Type type)
        {
            foreach (RtfTypeInfo item in typeInfoItems)
                if (item.Type == type)
                    return item.Members;
            
            RtfTypeInfo info = new RtfTypeInfo(type);
            typeInfoItems.Add(info);

            return info.Members;
        }
    }
}