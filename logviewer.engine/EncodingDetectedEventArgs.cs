// Created by: egr
// Created at: 01.10.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    /// Encoding detection results
    /// </summary>
    public sealed class EncodingDetectedEventArgs : EventArgs
    {
        private readonly Encoding encoding;

        /// <summary>
        /// Initializes new args instance
        /// </summary>
        /// <param name="encoding">Encoding detected</param>
        public EncodingDetectedEventArgs(Encoding encoding)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return this.encoding == null ? string.Empty : this.encoding.EncodingName;
        }
    }
}