using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace logviewer
{
    public class LogController
    {
        #region Constants and Fields

        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const int BomLength = 3; // BOM (Byte Order Mark)
        private const int MeanLogStringLength = 70;
        private const string SmallFileFormat = "{0} {1}";
        private const string StartMessagePattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";
        private static readonly Regex regex = new Regex(StartMessagePattern, RegexOptions.Compiled);

        private static readonly string[] sizes = new[]
            {
                "Bytes",
                "Kb",
                "Mb",
                "Gb",
                "Tb",
                "Pb",
                "Eb",
            };

        private readonly ILogView view;

        #endregion

        #region Constructors and Destructors

        public LogController(ILogView view)
        {
            this.view = view;
            Executive.SafeRun(Convert);
        }

        #endregion

        #region Public Properties

        public string HumanReadableLogSize { get; private set; }
        public long LogSize { get; private set; }

        #endregion

        #region Public Methods and Operators

        public static async Task<string> ReadLog(Stream stream)
        {
            var messages = new List<LogMessage>((int) stream.Length / MeanLogStringLength);
            var message = new LogMessage {Strings = new List<string>()};
            long length;
            using (var sr = new StreamReader(stream, Encoding.UTF8, true))
            {
                length = stream.Length;
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();

                    if (regex.IsMatch(line) && message.Strings.Count > 0)
                    {
                        messages.Add(message);
                        message.Strings = new List<string>();
                    }

                    message.Strings.Add(line);
                }
            }
            messages.Add(message);
            var sb = new StringBuilder((int) length + messages.Count * Environment.NewLine.Length);
            for (var i = messages.Count - 1; i >= 0; --i)
            {
                sb.Append(messages[i]);
            }
            return sb.ToString();
        }

        public string ReadLog(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return string.Empty;
                }
                var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (stream)
                {
                    LogSize = stream.Length;
                    CreateHumanReadableSize();

                    return ReadLog(stream).Result;
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        #endregion

        #region Methods

        private void Convert()
        {
            if (!File.Exists(view.LogPath))
            {
                return;
            }
            var stream = File.Open(view.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] b;
            using (stream)
            {
                b = new byte[BomLength];
                stream.Read(b, 0, BomLength);
            }
            if (b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) // Do not convert file that is already in UTF-8
            {
                return;
            }
            var srcEncoding = Encoding.GetEncoding("windows-1251");
            var log = File.ReadAllText(view.LogPath, srcEncoding);
            var asciiBytes = srcEncoding.GetBytes(log);
            var utf8Bytes = Encoding.Convert(srcEncoding, Encoding.UTF8, asciiBytes);
            var utf8 = Encoding.UTF8.GetString(utf8Bytes);
            File.WriteAllText(view.LogPath, utf8, Encoding.UTF8);
        }

        private void CreateHumanReadableSize()
        {
            var normalized = new FileSize((ulong) LogSize);
            if (normalized.Unit == SizeUnit.Bytes)
            {
                HumanReadableLogSize = string.Format(CultureInfo.CurrentCulture, SmallFileFormat, normalized.Bytes,
                                                     sizes[(int) normalized.Unit]);
            }
            else
            {
                HumanReadableLogSize = string.Format(CultureInfo.CurrentCulture, BigFileFormat, normalized.Value,
                                                     sizes[(int) normalized.Unit], normalized.Bytes,
                                                     sizes[(int) SizeUnit.Bytes]);
            }
        }

        #endregion
    }

    internal struct LogMessage
    {
        #region Constants and Fields

        internal IList<string> Strings;

        #endregion

        #region Public Methods and Operators

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var s in Strings)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }

        #endregion
    }
}