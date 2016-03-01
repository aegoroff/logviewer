// Created by: egr
// Created at: 14.09.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    ///     Reads log from file or stream
    /// </summary>
    public sealed class LogReader
    {
        private readonly ICharsetDetector detector;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private GrokMatcher excludeMatcher;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private GrokMatcher includeMatcher;
        private bool cancelled;

        /// <summary>
        ///     Initializes reader
        /// </summary>
        /// <param name="detector">Charset detector</param>
        /// <param name="matcher">Message matcher</param>
        public LogReader(ICharsetDetector detector, IMessageMatcher matcher) : this(detector, matcher.IncludeMatcher, matcher.ExcludeMatcher)
        {
        }
        
        /// <summary>
        ///     Initializes reader
        /// </summary>
        /// <param name="detector">Charset detector</param>
        /// <param name="includeMatcher">Matcher that defines message start</param>
        /// <param name="excludeMatcher">Matcher that filters row (row will not be included into any message)</param>
        public LogReader(ICharsetDetector detector, GrokMatcher includeMatcher, GrokMatcher excludeMatcher = null)
        {
            this.detector = detector;
            this.includeMatcher = includeMatcher;
            this.excludeMatcher = excludeMatcher;
        }

        /// <summary>
        ///     Occurs every 5% of log progress
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        ///     Occurs on starting log file encoding detection
        /// </summary>
        public event EventHandler EncodingDetectionStarted;

        /// <summary>
        ///     Occurs on finish log file encoding detection
        /// </summary>
        public event EventHandler<EncodingDetectedEventArgs> EncodingDetectionFinished;

        /// <summary>
        ///     Occurs on starting template compilation (template compilation may take a long time)
        /// </summary>
        public event EventHandler CompilationStarted;

        /// <summary>
        ///     Occurs on finish template compilation (template compilation may take a long time)
        /// </summary>
        public event EventHandler CompilationFinished;

        /// <summary>
        /// Cancels reading
        /// </summary>
        public void Cancel()
        {
            this.cancelled = true;
        }

        /// <summary>
        ///     Reads log from file
        /// </summary>
        /// <param name="logPath">Full path to file</param>
        /// <param name="encoding">File encoding. It will be detected automatically if null passed as parameter value</param>
        /// <param name="offset">file offset</param>
        /// <returns>Messages stream</returns>
        public IEnumerable<LogMessage> Read(string logPath, Encoding encoding = null, long offset = 0)
        {
            var length = new FileInfo(logPath).Length;
            if (length == 0)
            {
                yield break;
            }

            var mapName = Guid.NewGuid().ToString();
            using (
                var mmf = MemoryMappedFile.CreateFromFile(logPath, FileMode.Open, mapName, 0,
                    MemoryMappedFileAccess.Read))
            {
                if (encoding == null)
                {
                    this.EncodingDetectionStarted?.Invoke(this, new EventArgs());

                    using (var stream = mmf.CreateViewStream(0, length, MemoryMappedFileAccess.Read))
                    {
                        encoding = this.detector.Detect(stream);
                    }
                }
                this.EncodingDetectionFinished?.Invoke(this, new EncodingDetectedEventArgs(encoding));

                using (var stream = mmf.CreateViewStream(offset, length - offset, MemoryMappedFileAccess.Read))
                {
                    var enumerator = this.Read(stream, length, encoding).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }

        /// <summary>
        ///     Reads log from stream
        /// </summary>
        /// <param name="stream">Stream to read log from</param>
        /// <param name="length">Stream lendgh if applicable</param>
        /// <param name="encoding">Stream encoding</param>
        /// <returns>Messages stream</returns>
        public IEnumerable<LogMessage> Read(Stream stream, long length, Encoding encoding = null)
        {
            var decode = DecodeNeeded(encoding);
            var canSeek = stream.CanSeek;
            var sr = new StreamReader(stream, encoding ?? Encoding.UTF8);
            using (sr)
            {
                var total = length;
                var fraction = total / 20L;
                var signalCounter = 1;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var measureStart = 0L;
                var message = LogMessage.Create();
                var compiled = false;
                var line = sr.ReadLine();
                while (line != null && !this.cancelled)
                {
                    if (decode)
                    {
                        line = line.Convert(encoding, Encoding.UTF8);
                    }
                    if (this.excludeMatcher != null && this.excludeMatcher.Match(line))
                    {
                        line = sr.ReadLine();
                        continue;
                    }

                    // Occured on first row
                    if (!compiled)
                    {
                        this.CompilationStarted?.Invoke(this, new EventArgs());
                    }

                    var properties = this.includeMatcher.Parse(line);

                    // Occured only after first row
                    if (!compiled)
                    {
                        this.CompilationFinished?.Invoke(this, new EventArgs());
                    }

                    compiled = true;

                    // not null properties mean reading first message row that contains all meta information
                    // and start message criteria
                    if (properties != null)
                    {
                        if (message.HasHeader)
                        {
                            yield return message;
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
                    line = sr.ReadLine();

                    if (!canSeek || stream.Position < signalCounter * fraction || this.ProgressChanged == null)
                    {
                        continue;
                    }
                    var elapsed = stopWatch.Elapsed;
                    stopWatch.Restart();

                    measureStart = this.ReportProgress(stream, measureStart, elapsed, total, ref signalCounter);
                }
                if (!this.cancelled)
                {
                    // Add last message
                    yield return message;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long ReportProgress(Stream stream, long measureStart, TimeSpan elapsed, long total, ref int signalCounter)
        {
            var read = stream.Position - measureStart;
            measureStart = stream.Position;
            var speed = read / elapsed.TotalSeconds;

            ++signalCounter;
            var remain = Math.Abs(speed) < 0.001 ? 0 : (total - stream.Position) / speed;
            var progress = new LoadProgress
            {
                Speed = new FileSize((ulong) speed, true),
                Remainig = TimeSpan.FromSeconds(remain),
                Percent = stream.Position.PercentOf(total)
            };
            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress.Percent, progress));
            return measureStart;
        }

        private static bool DecodeNeeded(Encoding srcEncoding)
        {
            return srcEncoding != null && !srcEncoding.Equals(Encoding.UTF8) &&
                   !srcEncoding.Equals(Encoding.ASCII);
        }
    }
}