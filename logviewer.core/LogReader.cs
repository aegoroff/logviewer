// Created by: egr
// Created at: 14.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Ude;

namespace logviewer.core
{
    public sealed class LogReader
    {
        private const int BufferSize = 0xFFFFFF;
        private readonly Regex messageHead;

        public LogReader(string logPath, Regex messageHead)
        {
            this.LogPath = logPath;
            this.messageHead = messageHead;
            this.Length = new FileInfo(logPath).Length;
        }

        /// <summary>
        ///     Gets log file length in bytes
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        ///     Gets full path to log file
        /// </summary>
        public string LogPath { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler EncodingDetectionStarted;
        public event EventHandler<EncodingDetectedEventArgs> EncodingDetectionFinished;

        public Encoding Read(Action<LogMessage> onRead, Func<bool> canContinue, Encoding encoding = null, long offset = 0)
        {
            if (this.Length == 0)
            {
                return null;
            }

            Encoding srcEncoding;
            if (encoding != null)
            {
                srcEncoding = encoding;
            }
            else
            {
                if (this.EncodingDetectionStarted != null)
                {
                    this.EncodingDetectionStarted(this, new EventArgs());
                }
                var mapName = Guid.NewGuid().ToString();
                using (
                    var mmf = MemoryMappedFile.CreateFromFile(this.LogPath, FileMode.Open, mapName, 0,
                        MemoryMappedFileAccess.Read))
                {
                    using (var s = mmf.CreateViewStream(0, this.Length, MemoryMappedFileAccess.Read))
                    {
                        srcEncoding = SrcEncoding(s);
                    }
                }
            }
            if (this.EncodingDetectionFinished != null)
            {
                this.EncodingDetectionFinished(this, new EncodingDetectedEventArgs(srcEncoding));
            }
            var decode = DecodeNeeded(srcEncoding);
            var fs = new FileStream(this.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize);
            fs.Seek(offset, SeekOrigin.Begin);
            var stream = new BufferedStream(fs, BufferSize);
            var sr = new StreamReader(stream, srcEncoding ?? Encoding.UTF8);
            using (sr)
            {
                var message = LogMessage.Create();
                var total = this.Length;
                var fraction = total / 20L;
                var signalCounter = 1;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var measureStart = 0L;
                while (!sr.EndOfStream && canContinue())
                {
                    var line = sr.ReadLine();
                    if (decode)
                    {
                        line = line.Convert(srcEncoding, Encoding.UTF8);
                    }
                    if (line == null)
                    {
                        break;
                    }
                    if (this.messageHead.IsMatch(line))
                    {
                        onRead(message);
                        message = LogMessage.Create();
                    }

                    if (stream.Position >= signalCounter * fraction && this.ProgressChanged != null)
                    {
                        var elapsed = stopWatch.Elapsed;
                        var read = stream.Position - measureStart;
                        measureStart = stream.Position;
                        var speed = read / elapsed.TotalSeconds;
                        stopWatch.Restart();
                        ++signalCounter;
                        var remain = Math.Abs(speed) < 0.001 ? 0 : (total - stream.Position) / speed;
                        var progress = new LoadProgress
                        {
                            Speed = new FileSize((ulong)speed, true),
                            Remainig = TimeSpan.FromSeconds(remain),
                            Percent = (int)((stream.Position / (double)total) * 100)
                        };
                        this.ProgressChanged(this, new ProgressChangedEventArgs(progress.Percent, progress));
                    }

                    message.AddLine(line);
                }

                // Add last message
                onRead(message);
            }

            return srcEncoding;
        }

        private void Parse(string s)
        {
            ICharStream stream = new UnbufferedCharStream(new StringReader(s));
            GrokLexer gl = new GrokLexer(stream);
        }

        private static Encoding SrcEncoding(Stream stream)
        {
            Encoding srcEncoding = null;
            var detector = new CharsetDetector();
            detector.Feed(stream);
            detector.DataEnd();
            if (detector.Charset != null)
            {
                srcEncoding = Encoding.GetEncoding(detector.Charset);
            }
            return srcEncoding;
        }

        private static bool DecodeNeeded(Encoding srcEncoding)
        {
            return srcEncoding != null && !srcEncoding.Equals(Encoding.UTF8) &&
                   !srcEncoding.Equals(Encoding.ASCII);
        }
    }
}