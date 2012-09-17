using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

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
        private const int IssueMessageBatchMillisecondsTimeout = 150;
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

        public event EventHandler<LogMessageEventArgs> LogMessageRead;

        #region Constructors and Destructors

        public LogController(ILogView view)
        {
            this.view = view;
            this.Messages = new List<LogMessage>();
            Executive.SafeRun(this.Convert);
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
            Executive.SafeRun(this.ReadLogInternal, path);
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
                this.LogSize = reader.BaseStream.Length;
                this.CreateHumanReadableSize();

                this.ReadLogTask(reader);
            }
        }

        private void ReadLogTask(StreamReader sr)
        {
            this.Messages = new List<LogMessage>((int) this.LogSize / MeanLogStringLength);
            var message = new LogMessage { Strings = new List<string>() };

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                if (regex.IsMatch(line) && message.Strings.Count > 0)
                {
                    this.Messages.Add(message);
                    message.Strings = new List<string>();
                }

                message.Strings.Add(line);
            }
            this.Messages.Add(message);
            this.Messages.Reverse();
            var logMessageRead = this.LogMessageRead;
            if (logMessageRead == null)
            {
                return;
            }
            var sent = 0;
            var messageEventArgs = new LogMessageEventArgs(this.Messages.Count);
            const int portionSize = 200;
            var i = 0;
            foreach (var msg in this.Messages)
            {
                if (i++ < portionSize)
                {
                    messageEventArgs.Messages.Add(msg);
                }
                else
                {
                    sent += messageEventArgs.Messages.Count;
                    messageEventArgs.UpdatePercent(sent);
                    logMessageRead(this, messageEventArgs);
                    i = 0;
                    messageEventArgs = new LogMessageEventArgs(this.Messages.Count);
                    Thread.Sleep(IssueMessageBatchMillisecondsTimeout);
                }
            }
            sent += messageEventArgs.Messages.Count;
            messageEventArgs.UpdatePercent(sent);
            logMessageRead(this, messageEventArgs);
        }

        private void Convert()
        {
            if (!File.Exists(this.view.LogPath))
            {
                return;
            }
            var stream = File.Open(this.view.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
            var log = File.ReadAllText(this.view.LogPath, srcEncoding);
            var asciiBytes = srcEncoding.GetBytes(log);
            var utf8Bytes = Encoding.Convert(srcEncoding, Encoding.UTF8, asciiBytes);
            var utf8 = Encoding.UTF8.GetString(utf8Bytes);
            File.WriteAllText(this.view.LogPath, utf8, Encoding.UTF8);
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

        #endregion
    }

    public struct LogMessage
    {
        #region Constants and Fields

        internal IList<string> Strings;

        #endregion

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.Strings);
        }
    }

    /// <summary>
    ///     Just a simple class to message
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        private readonly int maxCount;

        public LogMessageEventArgs(int maxCount)
        {
            this.maxCount = maxCount;
            this.Messages = new List<LogMessage>();
            this.Percent = 0;
        }

        public IList<LogMessage> Messages { get; private set; }

        public int Percent { get; private set; }

        public void UpdatePercent(int sent)
        {
            this.Percent = (int)((sent / (double)this.maxCount) * 100);
        }
    }
}