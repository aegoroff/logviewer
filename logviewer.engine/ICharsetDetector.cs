// Created by: egr
// Created at: 19.11.2014
// © 2012-2014 Alexander Egorov

using System.IO;
using System.Text;

namespace logviewer.engine
{
    public interface ICharsetDetector
    {
        Encoding Detect(Stream stream);
    }
}