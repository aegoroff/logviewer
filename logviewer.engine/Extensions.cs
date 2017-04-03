// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.11.2014
// © 2012-2017 Alexander Egorov

using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    /// Useful engine extensions
    /// </summary>
    public static class Extensions
    {
        internal static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UnescapeString(this string escaped)
        {
            if (escaped.Length > 1 && (escaped.StartsWith("'") && escaped.EndsWith("'") || escaped.StartsWith("\"") && escaped.EndsWith("\""))) // Not L10N
            {
                return escaped.Substring(1, escaped.Length - 2).Replace("\\\"", "\"").Replace("\\'", "'"); // Not L10N
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
            var srcBytes = srcEncoding.GetBytes(line);
            var dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(dstBytes);
        }

        /// <summary>
        /// Returns percent of value specified from total
        /// </summary>
        /// <param name="value">Value to calculate percent</param>
        /// <param name="total">Value upper boundary</param>
        /// <returns>Percent of value from total parameter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PercentOf(this ulong value, ulong total)
        {
            return (int)((value / (double)total) * 100);
        }

        /// <summary>
        /// Returns percent of value specified from total
        /// </summary>
        /// <param name="value">Value to calculate percent</param>
        /// <param name="total">Value upper boundary</param>
        /// <returns>Percent of value from total parameter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PercentOf(this long value, long total)
        {
            var v = value > 0 ? (ulong)value : 0;
            return total == 0 ? 0 : v.PercentOf((ulong)total);
        }

        /// <summary>
        /// Returns percent of file size specified from total
        /// </summary>
        /// <param name="value">Value to calculate percent</param>
        /// <param name="total">Value upper boundary</param>
        /// <returns>Percent of value from total parameter</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PercentOf(this FileSize value, FileSize total)
        {
            return value.Bytes.PercentOf(total.Bytes);
        }
    }
}