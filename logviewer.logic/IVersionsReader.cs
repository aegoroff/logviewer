// Created by: egr
// Created at: 29.03.2014
// © 2012-2015 Alexander Egorov

using System;

namespace logviewer.logic
{
    public interface IVersionsReader
    {
        event EventHandler ReadCompleted;
        event EventHandler<VersionEventArgs> VersionRead;
        void ReadReleases();
    }
}