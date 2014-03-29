// Created by: egr
// Created at: 29.03.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Reflection;
using System.Threading;

namespace logviewer.core
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
        
        public Version CurrentVersion { get; private set; }

        public bool IsUpdatesAvaliable()
        {
            return this.IsUpdatesAvaliable(this.CurrentVersion);
        }

        public bool IsUpdatesAvaliable(Version current)
        {
            bool completed = false;
            bool result = false;
            this.reader.VersionRead += (sender, eventArgs) =>
            {
                if (result)
                {
                    return;
                }
                result = eventArgs.Version > current;
                if (result)
                {
                    this.LatestVersion = eventArgs.Version;
                }
            };
            this.reader.ReadCompleted += delegate { completed = true; };

            this.reader.ReadReleases();
            SpinWait.SpinUntil(() => completed);
            if (!result)
            {
                this.LatestVersion = current;
            }
            return result;
        }
    }
}