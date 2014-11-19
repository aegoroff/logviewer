// Created by: egr
// Created at: 19.11.2014
// © 2012-2014 Alexander Egorov

using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace logviewer.engine
{
    public static class Extensions
    {
        internal static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UnescapeString(this string escaped)
        {
            if (escaped.Length > 1 && (escaped.StartsWith("'") && escaped.EndsWith("'") || escaped.StartsWith("\"") && escaped.EndsWith("\"")))
            {
                return escaped.Substring(1, escaped.Length - 2).Replace("\\\"", "\"").Replace("\\'", "'");
            }
            return escaped;
        }

        /// <summary>
        ///     Decodes string into encoding specified
        /// </summary>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        /// <returns>Decoded string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string Convert(this string line, Encoding srcEncoding, Encoding dstEncoding)
        {
            byte[] srcBytes = srcEncoding.GetBytes(line);
            byte[] dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(dstBytes);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static int PercentOf(this ulong value, ulong total)
        {
            return (int)((value / (double)total) * 100);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static int PercentOf(this long value, long total)
        {
            var v = value > 0 ? (ulong)value : 0;
            return total == 0 ? 0 : v.PercentOf((ulong)total);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static int PercentOf(this FileSize value, FileSize total)
        {
            return value.Bytes.PercentOf(total.Bytes);
        }
    }
}