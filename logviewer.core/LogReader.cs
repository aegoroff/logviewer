using System;
using System.ComponentModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;
using Ude;

namespace logviewer.core
{
    public sealed class LogReader
    {
        private readonly Regex messageHead;
        private const int BufferSize = 0xFFFFF;

        public event ProgressChangedEventHandler ProgressChanged;

        public LogReader(string logPath, Regex messageHead)
        {
            this.LogPath = logPath;
            this.messageHead = messageHead;
            this.Length = new FileInfo(logPath).Length;
        }

        /// <summary>
        /// Gets log file length in bytes
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Gets full path to log file
        /// </summary>
        public string LogPath { get; private set; }

        public void Read(Action<LogMessage> onRead, Func<bool> canContinue, long offset = 0)
        {
            if (Length == 0)
            {
                return;
            }

            bool decode;
            Encoding srcEncoding;
            string mapName = Guid.NewGuid().ToString();
            using (
                var mmf = MemoryMappedFile.CreateFromFile(LogPath, FileMode.Open, mapName, 0,
                    MemoryMappedFileAccess.Read))
            {
                using (var stream = mmf.CreateViewStream(0, Length, MemoryMappedFileAccess.Read))
                {
                    srcEncoding = SrcEncoding(stream, out decode);
                }
            }

            using (var fs = File.Open(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                using (var stream = new BufferedStream(fs, BufferSize))
                {
                    var total = stream.Length;
                    var fraction = total / 20L;
                    var signalCounter = 1;

                    var sr = new StreamReader(stream, srcEncoding ?? Encoding.UTF8);
                    using (sr)
                    {
                        var message = LogMessage.Create();
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

                            if (stream.Position >= signalCounter * fraction)
                            {
                                ++signalCounter;
                                var percent = (stream.Position / (double) total) * 100;
                                if (ProgressChanged != null)
                                {
                                    ProgressChanged(this, new ProgressChangedEventArgs((int) percent, null));
                                }
                            }

                            message.AddLine(line);
                        }

                        // Add last message
                        onRead(message);
                    }
                }
            }
        }

        private static Encoding SrcEncoding(Stream stream, out bool decode)
        {
            Encoding srcEncoding = null;
            var detector = new CharsetDetector();
            detector.Feed(stream);
            detector.DataEnd();
            if (detector.Charset != null)
            {
                srcEncoding = Encoding.GetEncoding(detector.Charset);
            }
            decode = srcEncoding != null && !srcEncoding.Equals(Encoding.UTF8) &&
                     !srcEncoding.Equals(Encoding.ASCII);
            return srcEncoding;
        }
    }
}