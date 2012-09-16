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
            Messages = new List<LogMessage>();
            Executive.SafeRun(Convert);
        }

        #endregion

        #region Public Properties

        public string HumanReadableLogSize { get; private set; }
        public long LogSize { get; private set; }

        public List<LogMessage> Messages { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void ReadLog(string path)
        {
            Executive.SafeRun(ReadLogInternal, path);
        }

        #endregion

        #region Methods

        private void ReadLogInternal(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            var reader = File.OpenText(path);
            using (reader)
            {
                LogSize = reader.BaseStream.Length;
                CreateHumanReadableSize();

                ReadLogTask(reader);
            }
        }

        private void ReadLogTask(StreamReader sr)
        {
            Messages = new List<LogMessage>((int) LogSize / MeanLogStringLength);
            var message = new LogMessage { Strings = new List<string>() };

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                if (regex.IsMatch(line) && message.Strings.Count > 0)
                {
                    Messages.Add(message);
                    message.Strings = new List<string>();
                }

                message.Strings.Add(line);
            }
            Messages.Add(message);
            Messages.Reverse();
        }

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

    public struct LogMessage
    {
        #region Constants and Fields

        internal IList<string> Strings;

        #endregion
    }
}