﻿// Created by: egr
// Created at: 29.03.2014
// © 2012-2016 Alexander Egorov

using System;
using logviewer.logic.Annotations;
using logviewer.logic.models;

namespace logviewer.logic
{
    public interface IVersionsReader
    {
        [PublicAPI]
        void ReadReleases();

        [PublicAPI]
        IDisposable Subscribe(Action<VersionModel> onNext, Action onCompleted);
    }
}