// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 29.03.2014
// © 2012-2017 Alexander Egorov

using System;
using logviewer.logic.Annotations;
using logviewer.logic.models;

namespace logviewer.logic
{
    public interface IVersionsReader : IDisposable
    {
        [PublicAPI]
        void ReadReleases();

        [PublicAPI]
        IDisposable Subscribe(Action<VersionModel> onNext, Action onCompleted);
    }
}
