// ***********************************************************************
// <author>Alexander Egorov</author>
//
// <summary>  </summary>
//
// <copyright company="Comindware">    
//    Copyright (c) Comindware 2010-2014. All rights reserved.   
// </copyright>
// ***********************************************************************

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