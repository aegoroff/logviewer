// Created by: egr
// Created at: 29.03.2014
// © 2012-2016 Alexander Egorov

using System;

namespace logviewer.logic.models
{
    public sealed class VersionModel
    {
        public Version Version { get; private set; }
        public string Url { get; private set; }

        public VersionModel(Version version, string url)
        {
            this.Version = version;
            this.Url = url;
        }
    }
}