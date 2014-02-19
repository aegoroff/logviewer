// Created by: egr
// Created at: 01.10.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Text;

namespace logviewer.core
{
    public sealed class EncodingDetectedEventArgs : EventArgs
    {
        private readonly Encoding encoding;

        public EncodingDetectedEventArgs(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override string ToString()
        {
            return this.encoding == null ? string.Empty : this.encoding.EncodingName;
        }
    }
}