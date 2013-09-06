using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;
using Ude;

namespace logviewer.core
{
    public static class Extensions
    {
        #region Delegates

        internal delegate bool CancelPredicate();

        #endregion

        #region Methods

        internal static string ConvertToUtf8(this string originalPath, CancelPredicate predicate)
        {
            var fi = new FileInfo(originalPath);
            if (fi.Length == 0)
            {
                return originalPath;
            }
            using (
                var mmf = MemoryMappedFile.CreateFromFile(originalPath, FileMode.Open,
                    Guid.NewGuid().ToString(), 0, MemoryMappedFileAccess.Read))
            {
                using (
                    var originalStream = mmf.CreateViewStream(0, fi.Length,
                        MemoryMappedFileAccess.Read))
                {
                    Encoding srcEncoding;
                    var detector = new CharsetDetector();
                    detector.Feed(originalStream);
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

                    originalStream.Seek(0, SeekOrigin.Begin);

                    var convertedPath = Path.GetTempFileName();
                    using (var convertedStream = File.Create(convertedPath))
                    {
                        convertedStream.WriteByte(0xEF);
                        convertedStream.WriteByte(0xBB);
                        convertedStream.WriteByte(0xBF);

                        var sr = new StreamReader(originalStream, srcEncoding);
                        using (sr)
                        {
                            while (!sr.EndOfStream && predicate())
                            {
                                var line = sr.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }
                                convertedStream.WriteLine(line, srcEncoding, Encoding.UTF8);
                            }
                        }
                    }
                    return convertedPath;
                }
            }
        }

        internal static Regex ToMarker(this string marker)
        {
            return new Regex(marker, RegexOptions.Compiled);
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
            byte[] srcBytes = srcEncoding.GetBytes(line);
            byte[] dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            string dst = dstEncoding.GetString(dstBytes);
            dstBytes = dstEncoding.GetBytes(dst);
            stream.Write(dstBytes, 0, dstBytes.Length);
        }

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
            byte[] newLineBytes = dstEncoding.GetBytes(Environment.NewLine);
            stream.Write(newLineBytes, 0, newLineBytes.Length);
        }

        #endregion
    }
}