// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.10.2014
// © 2012-2016 Alexander Egorov

using System.IO;
using System.Text;
using logviewer.logic.support;
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