// Created by: egr
// Created at: 29.03.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Threading;

namespace logviewer.core
{
    public class UpdatesChecker
    {
        private readonly IVersionsReader reader;

        public UpdatesChecker(IVersionsReader reader)
        {
            this.reader = reader;
        }

        public bool IsUpdatesAvaliable(Version current)
        {
            var completed = false;
            var result = false;
            this.reader.VersionRead += (sender, eventArgs) =>
            {
                if (!result)
                {
                    result = eventArgs.Version > current;
                }
            };
            this.reader.ReadCompleted += delegate { completed = true; };

            this.reader.ReadReleases();
            SpinWait.SpinUntil(() => completed);
            return result;
        }
    }
}