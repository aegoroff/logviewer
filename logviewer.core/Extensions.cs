using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace logviewer.core
{
    public static class Extensions
    {
        /// <summary>
        ///     Writes string line specified into stream with recoding if necessary
        /// </summary>
        /// <param name="stream"> Stream to write to </param>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        public static void WriteLine(this Stream stream, string line, Encoding srcEncoding, Encoding dstEncoding)
        {
            stream.Write(line, srcEncoding, dstEncoding);
            var newLineBytes = dstEncoding.GetBytes(Environment.NewLine);
            stream.Write(newLineBytes, 0, newLineBytes.Length);
        }

        /// <summary>
        ///     Writes string specified into stream with recoding if necessary
        /// </summary>
        /// <param name="stream"> Stream to write to </param>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        public static void Write(this Stream stream, string line, Encoding srcEncoding, Encoding dstEncoding)
        {
            var srcBytes = srcEncoding.GetBytes(line);
            var dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            var dst = dstEncoding.GetString(dstBytes);
            dstBytes = dstEncoding.GetBytes(dst);
            stream.Write(dstBytes, 0, dstBytes.Length);
        }

        internal static Regex ToMarker(this string marker)
        {
            return new Regex(marker, RegexOptions.Compiled);
        }
    }
}