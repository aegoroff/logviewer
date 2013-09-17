// Created by: egr
// Created at: 20.09.2012
// © 2012-2013 Alexander Egorov

using System.Text;
using System.Text.RegularExpressions;

namespace logviewer.core
{
    public static class Extensions
    {
        internal static Regex ToMarker(this string marker)
        {
            return new Regex(marker, RegexOptions.Compiled);
        }

        /// <summary>
        ///     Decodes string into encoding specified
        /// </summary>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        /// <returns>Decoded string</returns>
        internal static string Convert(this string line, Encoding srcEncoding, Encoding dstEncoding)
        {
            byte[] srcBytes = srcEncoding.GetBytes(line);
            byte[] dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(dstBytes);
        }
    }
}