// Created by: egr
// Created at: 29.03.2014
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.core
{
    public interface IVersionsReader
    {
        event EventHandler ReadCompleted;
        event EventHandler<VersionEventArgs> VersionRead;
        void ReadReleases();
    }
}