using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ude;

namespace logviewer.core
{
    public static class Extensions
    {
        /// <summary>
        ///     Writes string line specified into stream with converting if necessary
        /// </summary>
        /// <param name="stream"> Stream to write to </param>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        private static void WriteLine(this Stream stream, string line, Encoding srcEncoding, Encoding dstEncoding)
        {
            stream.Write(line, srcEncoding, dstEncoding);
            var newLineBytes = dstEncoding.GetBytes(Environment.NewLine);
            stream.Write(newLineBytes, 0, newLineBytes.Length);
        }

        /// <summary>
        ///     Writes string specified into stream with converting if necessary
        /// </summary>
        /// <param name="stream"> Stream to write to </param>
        /// <param name="line"> Source string </param>
        /// <param name="srcEncoding"> Source string encoding </param>
        /// <param name="dstEncoding"> Destination encoding </param>
        private static void Write(this Stream stream, string line, Encoding srcEncoding, Encoding dstEncoding)
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

        internal delegate bool CancelPredicate();

        internal static string ConvertToUtf8(this string originalPath, CancelPredicate predicate = null)
        {
            var stream = File.Open(originalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            
            using (stream)
            {
                Encoding srcEncoding;
                var detector = new CharsetDetector();
                detector.Feed(stream);
                detector.DataEnd();
                if (detector.Charset != null)
                {
                    srcEncoding = Encoding.GetEncoding(detector.Charset);
                }
                else
                {
                    return originalPath;
                }

                if (srcEncoding.Equals(Encoding.UTF8) || srcEncoding.Equals(Encoding.ASCII))
                {
                    return originalPath;
                }

                stream.Seek(0, SeekOrigin.Begin);

                var convertedPath = Path.GetTempFileName();
                var f = File.Create(convertedPath);
                using (f)
                {
                    f.WriteByte(0xEF);
                    f.WriteByte(0xBB);
                    f.WriteByte(0xBF);

                    var sr = new StreamReader(stream, srcEncoding);
                    using (sr)
                    {
                        while (!sr.EndOfStream && (predicate == null || predicate()))
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            f.WriteLine(line, srcEncoding, Encoding.UTF8);
                        }
                    }
                }
                return convertedPath;
            }
        }
    }
}