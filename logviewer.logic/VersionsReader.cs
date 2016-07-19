// Created by: egr
// Created at: 29.03.2014
// © 2012-2016 Alexander Egorov

using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using logviewer.logic.models;
using logviewer.logic.support;
using Octokit;

namespace logviewer.logic
{
    internal class VersionsReader : IVersionsReader
    {
        private readonly string account;
        private readonly string project;
        private readonly Regex versionRegexp = new Regex(@"^.*(\d+\.\d+\.\d+\.\d+)\.(exe|msi)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string DownloadUrlTemplate = @"http://github.com/{0}/{1}/releases/download/{2}/{3}";

        private readonly Subject<VersionModel> subject;


        public VersionsReader(string account, string project)
        {
            this.account = account;
            this.project = project;
            this.subject = new Subject<VersionModel>();
        }

        public void ReadReleases()
        {
            try
            {
                var github = new GitHubClient(new ProductHeaderValue(this.project));
                var releases = github.Repository.Release.GetAll(this.account, this.project);
                releases.Wait();
                foreach (var release in releases.Result)
                {
                    try
                    {
                        var assets = github.Repository.Release.GetAllAssets(this.account, this.project, release.Id);
                        assets.Wait();
                        foreach (var m in from releaseAsset in assets.Result
                            select this.versionRegexp.Match(releaseAsset.Name)
                            into match
                            where match.Success
                            select match)
                        {
                            var url = string.Format(DownloadUrlTemplate, this.account, this.project, release.Name, m.Value);
                            var version = new Version(m.Groups[1].Captures[0].Value);
                            this.subject.OnNext(new VersionModel(version, url));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Instance.Warn(e.Message, e);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
            finally
            {
                this.subject.OnCompleted();
            }
        }

        public IDisposable Subscribe(Action<VersionModel> onNext, Action onCompleted)
        {
            return this.subject.Subscribe(onNext, onCompleted);
        }
    }
}
