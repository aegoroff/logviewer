// Created by: egr
// Created at: 04.10.2014
// © 2012-2015 Alexander Egorov

using System.IO;
using System.Text;
using Ude;
using ICharsetDetector = logviewer.engine.ICharsetDetector;

namespace logviewer.logic
{
    public class LogCharsetDetector : ICharsetDetector
    {
        public Encoding Detect(Stream stream)
        {
            var detector = new CharsetDetector();
            detector.Feed(stream);
            detector.DataEnd();
            return detector.Charset.Return(Encoding.GetEncoding, null);
        }
    }
}