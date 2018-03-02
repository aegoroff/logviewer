// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 01.10.2013
// © 2012-2018 Alexander Egorov

using System;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    /// Encoding detection results
    /// </summary>
    public sealed class EncodingDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets detected <see cref="Encoding"/> instance
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Initializes new args instance
        /// </summary>
        /// <param name="encoding">Encoding detected</param>
        public EncodingDetectedEventArgs(Encoding encoding) => this.Encoding = encoding;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => this.Encoding?.EncodingName ?? string.Empty;
    }
}
