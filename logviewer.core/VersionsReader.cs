// Created by: egr
// Created at: 29.03.2014
// © 2012-2014 Alexander Egorov

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Octokit;

namespace logviewer.core
{
    public class VersionsReader : IVersionsReader
    {
        private readonly string account;
        private readonly string project;
        const string Account = "aegoroff";
        const string Project = "logviewer";
        readonly Regex versionRegexp = new Regex(@"^.*(\d+\.\d+\.\d+\.\d+)\.(exe|msi)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string DownloadUrlTemplate = "http://github.com/{0}/{1}/releases/download/{2}/{3}";

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
                if (this.VersionRead == null)
                {
                    return;
                }
                var github = new GitHubClient(new ProductHeaderValue(this.project));
                var releases = await github.Release.GetAll(this.account, this.project);
                foreach (var release in releases)
                {
                    try
                    {
                        var assets = await github.Release.GetAssets(this.account, this.project, release.Id);
                        foreach (var m in from releaseAsset in assets
                            select this.versionRegexp.Match(releaseAsset.Name)
                            into match
                            where match.Success
                            select match)
                        {
                            var url = string.Format(DownloadUrlTemplate, this.account, this.project, release.Name, m.Value);
                            var version = new Version(m.Groups[1].Captures[0].Value);
                            this.VersionRead(this, new VersionEventArgs(version, url));
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
}
