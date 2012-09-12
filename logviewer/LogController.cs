using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace logviewer
{
    public class LogController
    {
        private const int MeanLogStringLength = 70;
        private const int BomLength = 3; // BOM (Byte Order Mark)
        private const string StartMessagePattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";
        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const string SmallFileFormat = "{0} {1}";
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

        public LogController(ILogView view)
        {
            this.view = view;
            Executive.SafeRun(this.Convert);
        }

        public long LogSize { get; private set; }

        public string HumanReadableLogSize { get; private set; }

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
                    this.LogSize = stream.Length;
                    this.CreateHumanReadableSize();

                    return ReadLog(stream);
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private void CreateHumanReadableSize()
        {
            var normalized = new FileSize((ulong) this.LogSize);
            if (normalized.Unit == SizeUnit.Bytes)
            {
                this.HumanReadableLogSize = string.Format(CultureInfo.CurrentCulture, SmallFileFormat, normalized.Bytes,
                                                          sizes[(int) normalized.Unit]);
            }
            else
            {
                this.HumanReadableLogSize = string.Format(CultureInfo.CurrentCulture, BigFileFormat, normalized.Value,
                                                          sizes[(int) normalized.Unit], normalized.Bytes,
                                                          sizes[(int) SizeUnit.Bytes]);
            }
        }

        public static string ReadLog(Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8, true))
            {
                var messages = new List<LogMessage>((int) stream.Length / MeanLogStringLength);
                var message = new LogMessage { Strings = new List<string>() };
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (regex.IsMatch(line) && message.Strings.Count > 0)
                    {
                        messages.Add(message);
                        message.Strings = new List<string>();
                    }

                    message.Strings.Add(line);
                }
                messages.Add(message);
                var sb = new StringBuilder((int) stream.Length + messages.Count * Environment.NewLine.Length);
                for (int i = messages.Count - 1; i >= 0; --i)
                {
                    sb.Append(messages[i]);
                }
                return sb.ToString();
            }
        }

        private void Convert()
        {
            if (!File.Exists(this.view.LogPath))
            {
                return;
            }
            FileStream stream = File.Open(this.view.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
            Encoding srcEncoding = Encoding.GetEncoding("windows-1251");
            string log = File.ReadAllText(this.view.LogPath, srcEncoding);
            byte[] asciiBytes = srcEncoding.GetBytes(log);
            byte[] utf8Bytes = Encoding.Convert(srcEncoding, Encoding.UTF8, asciiBytes);
            string utf8 = Encoding.UTF8.GetString(utf8Bytes);
            File.WriteAllText(this.view.LogPath, utf8, Encoding.UTF8);
        }
    }

    internal struct LogMessage
    {
        internal IList<string> Strings;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in this.Strings)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }
    }
}