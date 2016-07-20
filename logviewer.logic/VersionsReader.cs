// Created by: egr
// Created at: 29.03.2014
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using logviewer.logic.models;
using logviewer.logic.support;
using Octokit;

namespace logviewer.logic
{
    internal class VersionsReader : IVersionsReader, IDisposable
    {
        private readonly string account;
        private readonly string project;
        private readonly GitHubClient github;

        private readonly Regex versionRegexp = new Regex(@"^.*(\d+\.\d+\.\d+\.\d+)\.(exe|msi)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const string DownloadUrlTemplate = @"http://github.com/{0}/{1}/releases/download/{2}/{3}";

        private readonly Subject<VersionModel> subject;


        public VersionsReader(string account, string project)
        {
            this.account = account;
            this.project = project;
            this.subject = new Subject<VersionModel>();
            this.github = new GitHubClient(new ProductHeaderValue(this.project));
        }

        public void ReadReleases()
        {
            try
            {
                var releases = this.github.Repository.Release.GetAll(this.account, this.project);
                releases.ContinueWith(this.OnReleasesListCompleted, TaskContinuationOptions.NotOnFaulted);
                releases.ContinueWith(task =>
                {
                    this.subject.OnCompleted();
                    Log.Instance.Error(task.Exception?.InnerException.Message, task.Exception?.InnerException);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
                this.subject.OnCompleted();
            }
        }

        private void OnReleasesListCompleted(Task<IReadOnlyList<Release>> task)
        {
            for (var i = 0; i < task.Result.Count; i++)
            {
                try
                {
                    var release = task.Result[i];
                    var assets = this.github.Repository.Release.GetAllAssets(this.account, this.project, release.Id);
                    var asset = assets.ContinueWith(this.OnAssetComplete, release);
                    if (i == task.Result.Count - 1)
                    {
                        asset.ContinueWith(t => this.subject.OnCompleted());
                    }
                }
                catch (Exception e)
                {
                    Log.Instance.Warn(e.Message, e);
                    if (i == task.Result.Count - 1)
                    {
                        this.subject.OnCompleted();
                    }
                }
            }
        }

        private void OnAssetComplete(Task<IReadOnlyList<ReleaseAsset>> task, object state)
        {
            var release = (Release) state;
            foreach (var m in from releaseAsset in task.Result
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

        public IDisposable Subscribe(Action<VersionModel> onNext, Action onCompleted)
        {
            return this.subject.Subscribe(onNext, onCompleted);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.subject.Dispose();
            }
        }
    }
}