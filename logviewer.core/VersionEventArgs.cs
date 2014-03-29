// Created by: egr
// Created at: 29.03.2014
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    public sealed class VersionEventArgs : EventArgs
    {
        public Version Version { get; private set; }
        public string Url { get; set; }

        public VersionEventArgs(Version version, string url)
        {
            this.Version = version;
            this.Url = url;
        }
    }
}