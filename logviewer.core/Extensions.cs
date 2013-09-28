// Created by: egr
// Created at: 20.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.Linq;
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

        static int CountDigits(long num)
        {
            return (num /= 10) > 0 ? 1 + CountDigits(num) : 1;
        }

        public static string FormatString(this long value)
        {
            var digits = CountDigits(value);

            var sb = new StringBuilder();
            for (int i = 0; i < digits; i++)
            {
                sb.Append("#");
                if ((i + 1) % 3 == 0 && i + 1 < digits)
                {
                    sb.Append(" ");
                }
            }
            return new string(sb.ToString().Reverse().ToArray());
        }
    }
}