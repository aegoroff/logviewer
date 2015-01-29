// Created by: egr
// Created at: 04.10.2014
// © 2012-2015 Alexander Egorov

using System.IO;
using System.Text;
using Ude;
using ICharsetDetector = logviewer.engine.ICharsetDetector;

namespace logviewer.core
{
    public class LogCharsetDetector : ICharsetDetector
    {
        public Encoding Detect(Stream stream)
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
    }
}