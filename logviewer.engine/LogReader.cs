// Created by: egr
// Created at: 14.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using Ude;

namespace logviewer.engine
{
    public sealed class LogReader
    {
        private const int BufferSize = 0xFFFFFF;
        private readonly GrokMatcher matcher;
        private readonly GrokMatcher filter;

        public LogReader(string logPath, GrokMatcher matcher, GrokMatcher filter = null)
        {
            this.LogPath = logPath;
            this.matcher = matcher;
            this.filter = filter;
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
        public event EventHandler CompilationStarted;
        public event EventHandler CompilationFinished;

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
                var total = this.Length;
                var fraction = total / 20L;
                var signalCounter = 1;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var measureStart = 0L;
                var message = LogMessage.Create();
                var compiled = false;
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
                    if (this.filter != null && this.filter.Match(line))
                    {
                        continue;
                    }

                    // Occured on first row
                    if (!compiled)
                    {
                        if (this.CompilationStarted != null)
                        {
                            this.CompilationStarted(this, new EventArgs());
                        }
                    }

                    var properties = this.matcher.Parse(line);

                    // Occured only after first row
                    if (!compiled)
                    {
                        if (this.CompilationFinished != null)
                        {
                            this.CompilationFinished(this, new EventArgs());
                        }
                    }

                    compiled = true;

                    if (properties != null)
                    {
                        if (message.HasHeader)
                        {
                            onRead(message);
                            message = LogMessage.Create();
                        }
                        else
                        {
                            // Remove trash from prev bad match
                            message.Clear();
                        }
                        message.AddProperties(properties);
                    }
                    message.AddLine(line);

                    if (stream.Position < signalCounter*fraction || this.ProgressChanged == null)
                    {
                        continue;
                    }
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
                        Percent = stream.Position.PercentOf(total)
                    };
                    this.ProgressChanged(this, new ProgressChangedEventArgs(progress.Percent, progress));
                }
                // Add last message
                onRead(message);
            }

            return srcEncoding;
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