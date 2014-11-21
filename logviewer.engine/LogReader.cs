// Created by: egr
// Created at: 14.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    /// Reads log from file or stream
    /// </summary>
    public sealed class LogReader
    {
        private const int BufferSize = 0xFFFFFF;
        private readonly ICharsetDetector detector;
        private readonly GrokMatcher matcher;
        private readonly GrokMatcher filter;

        /// <summary>
        /// Initializes reader
        /// </summary>
        /// <param name="detector">Charset detector</param>
        /// <param name="matcher">Message matcher</param>
        /// <param name="filter">Message filter if applicable</param>
        public LogReader(ICharsetDetector detector, GrokMatcher matcher, GrokMatcher filter = null)
        {
            this.detector = detector;
            this.matcher = matcher;
            this.filter = filter;
        }

        /// <summary>
        /// Occurs every 5% of log progress
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;
        
        /// <summary>
        /// Occurs on starting log file encoding detection
        /// </summary>
        public event EventHandler EncodingDetectionStarted;
        
        /// <summary>
        /// Occurs on finish log file encoding detection
        /// </summary>
        public event EventHandler<EncodingDetectedEventArgs> EncodingDetectionFinished;
        
        /// <summary>
        /// Occurs on starting template compilation (template compilation may take a long time)
        /// </summary>
        public event EventHandler CompilationStarted;
        
        /// <summary>
        /// Occurs on finish template compilation (template compilation may take a long time)
        /// </summary>
        public event EventHandler CompilationFinished;

        /// <summary>
        /// Reads log from file
        /// </summary>
        /// <param name="logPath">Full path to file</param>
        /// <param name="onRead">On message complete action</param>
        /// <param name="canContinue">Continue validator</param>
        /// <param name="encoding">File encoding</param>
        /// <param name="offset">file offset</param>
        /// <returns>Detected file encoding</returns>
        public Encoding Read(string logPath, Action<LogMessage> onRead, Func<bool> canContinue, Encoding encoding = null, long offset = 0)
        {
            var length = new FileInfo(logPath).Length;
            if (length == 0)
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
                    var mmf = MemoryMappedFile.CreateFromFile(logPath, FileMode.Open, mapName, 0,
                        MemoryMappedFileAccess.Read))
                {
                    using (var s = mmf.CreateViewStream(0, length, MemoryMappedFileAccess.Read))
                    {
                        srcEncoding = this.detector.Detect(s);
                    }
                }
            }
            if (this.EncodingDetectionFinished != null)
            {
                this.EncodingDetectionFinished(this, new EncodingDetectedEventArgs(srcEncoding));
            }

            var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize);
            fs.Seek(offset, SeekOrigin.Begin);
            var stream = new BufferedStream(fs, BufferSize);
            this.Read(stream, length, onRead, canContinue, srcEncoding);

            return srcEncoding;
        }

        /// <summary>
        /// Reads log from stream
        /// </summary>
        /// <param name="stream">Stream to read log from</param>
        /// <param name="length">Stream lendgh if applicable</param>
        /// <param name="onRead">On message complete action</param>
        /// <param name="canContinue">Continue validator</param>
        /// <param name="encoding">Stream encoding</param>
        /// <returns>Current stream position</returns>
        public long Read(Stream stream, long length, Action<LogMessage> onRead, Func<bool> canContinue, Encoding encoding = null)
        {
            var decode = DecodeNeeded(encoding);
            var canSeek = stream.CanSeek;
            var sr = new StreamReader(stream, encoding ?? Encoding.UTF8);
            var result = 0L;
            using (sr)
            {
                var total = length;
                var fraction = total/20L;
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
                        line = line.Convert(encoding, Encoding.UTF8);
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

                    if (!canSeek || stream.Position < signalCounter * fraction || this.ProgressChanged == null)
                    {
                        continue;
                    }
                    var elapsed = stopWatch.Elapsed;
                    var read = stream.Position - measureStart;
                    measureStart = stream.Position;
                    var speed = read/elapsed.TotalSeconds;
                    stopWatch.Restart();
                    ++signalCounter;
                    var remain = Math.Abs(speed) < 0.001 ? 0 : (total - stream.Position)/speed;
                    var progress = new LoadProgress
                    {
                        Speed = new FileSize((ulong) speed, true),
                        Remainig = TimeSpan.FromSeconds(remain),
                        Percent = stream.Position.PercentOf(total)
                    };
                    this.ProgressChanged(this, new ProgressChangedEventArgs(progress.Percent, progress));
                }
                if (canSeek)
                {
                    result = stream.Position;
                }
                // Add last message
                onRead(message);
            }
            return result;
        }

        private static bool DecodeNeeded(Encoding srcEncoding)
        {
            return srcEncoding != null && !srcEncoding.Equals(Encoding.UTF8) &&
                   !srcEncoding.Equals(Encoding.ASCII);
        }
    }
}