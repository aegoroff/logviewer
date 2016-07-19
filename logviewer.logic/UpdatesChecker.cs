// Created by: egr
// Created at: 29.03.2014
// © 2012-2016 Alexander Egorov

using System;
using System.Reflection;
using System.Threading;

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

        public bool IsUpdatesAvaliable()
        {
            return this.IsUpdatesAvaliable(this.CurrentVersion);
        }

        public bool IsUpdatesAvaliable(Version current)
        {
            var completed = false;
            var result = false;
            this.LatestVersion = current;
            this.reader.Subscribe(args =>
            {
                result = args.Version > current;
                if (!result || this.LatestVersion > args.Version)
                {
                    return;
                }
                this.LatestVersion = args.Version;
                this.LatestVersionUrl = args.Url;
            }, () => completed = true);

            this.reader.ReadReleases();
            SpinWait.SpinUntil(() => completed);
            return result;
        }
    }
}