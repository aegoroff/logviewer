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
        private readonly string path;
        private readonly Regex messageHead;
        private readonly long offset;

        public event ProgressChangedEventHandler ProgressChanged;

        public LogReader(string path, Regex messageHead, long offset = 0)
        {
            this.path = path;
            this.messageHead = messageHead;
            this.offset = offset;
        }

        public long Length
        {
            get { return new FileInfo(path).Length; }
        }

        public void Read(Action<LogMessage> onRead, Func<bool> canContinue)
        {
            if (Length == 0)
            {
                return;
            }

            bool decode;
            Encoding srcEncoding;
            string mapName = Guid.NewGuid().ToString();
            using(var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open, mapName, 0, MemoryMappedFileAccess.Read))
            {
                using (var stream = mmf.CreateViewStream(0, Length, MemoryMappedFileAccess.Read))
                {
                    srcEncoding = SrcEncoding(stream, out decode);
                }
            }

            using(var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open, mapName, 0, MemoryMappedFileAccess.Read))
            {
                using (var stream = mmf.CreateViewStream(this.offset, Length, MemoryMappedFileAccess.Read))
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
                            if (line == null)
                            {
                                break;
                            }
                            if (decode)
                            {
                                line = line.Convert(srcEncoding, Encoding.UTF8);
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