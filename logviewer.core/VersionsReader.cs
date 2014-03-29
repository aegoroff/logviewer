// Created by: egr
// Created at: 29.03.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Text.RegularExpressions;
using Octokit;

namespace logviewer.core
{
    public class VersionsReader
    {
        private readonly string account;
        private readonly string project;
        const string Account = "aegoroff";
        const string Project = "logviewer";
        readonly Regex versionRegexp = new Regex(@"^.*(\d+\.\d+\.\d+\.\d+)\.exe$", RegexOptions.Compiled);

        public event EventHandler ReadCompleted;
        public event EventHandler<VersionEventArgs> VersionRead;

        public VersionsReader() : this(Account, Project)
        {
        }
        
        public VersionsReader(string account, string project)
        {
            this.account = account;
            this.project = project;
        }

        public async void ReadReleases()
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue(this.project));
                var releases = await github.Release.GetAll(this.account, this.project);
                foreach (var release in releases)
                {
                    try
                    {
                        var assets = await github.Release.GetAssets(this.account, this.project, release.Id);
                        foreach (var releaseAsset in assets)
                        {
                            Console.WriteLine(releaseAsset.Name);
                            var match = versionRegexp.Match(releaseAsset.Name);
                            if (this.VersionRead == null || !match.Success)
                            {
                                continue;
                            }
                            var v = new Version(match.Groups[1].Captures[0].Value);
                            this.VersionRead(this, new VersionEventArgs(v));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Instance.Warn(e.Message, e);
                    }
                }
            }
            finally
            {
                if (this.ReadCompleted != null)
                {
                    this.ReadCompleted(this, new EventArgs());
                }
            }
        }
    }

    public sealed class VersionEventArgs : EventArgs
    {
        public Version Version { get; private set; }

        public VersionEventArgs(Version version)
        {
            this.Version = version;
        }
    }
}
