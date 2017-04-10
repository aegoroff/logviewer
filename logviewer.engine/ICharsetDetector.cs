// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.11.2014
// © 2012-2017 Alexander Egorov

using System.IO;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    /// Represents charset detection class interface
    /// </summary>
    public interface ICharsetDetector
    {
        /// <summary>
        /// Detect specified stream encoding
        /// </summary>
        /// <param name="stream">String to detect encoding for</param>
        /// <returns>Encoding detected or null</returns>
        Encoding Detect(Stream stream);
    }
}
