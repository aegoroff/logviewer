// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 29.03.2014
// © 2012-2018 Alexander Egorov

using System;
using System.Reflection;
using logviewer.logic.Annotations;

namespace logviewer.logic
{
    public class UpdatesChecker
    {
        private readonly IVersionsReader reader;

        public UpdatesChecker(IVersionsReader reader)
        {
            this.reader = reader;
            this.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public Version LatestVersion { get; private set; }

        public string LatestVersionUrl { get; private set; }

        public Version CurrentVersion { get; }

        [PublicAPI]
        public void CheckUpdatesAvaliable(Action<bool> onComplete) => this.CheckUpdatesAvaliable(onComplete, this.CurrentVersion);

        [PublicAPI]
        public void CheckUpdatesAvaliable(Action<bool> onComplete, Version current)
        {
            var result = false;
            this.LatestVersion = current;
            this.reader.Subscribe(args =>
            {
                if (this.LatestVersion > args.Version)
                {
                    return;
                }

                result = true;
                this.LatestVersion = args.Version;
                this.LatestVersionUrl = args.Url;
            }, () => onComplete(result));

            this.reader.ReadReleases();
        }
    }
}
