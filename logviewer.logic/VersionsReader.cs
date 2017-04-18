// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 29.03.2014
// © 2012-2017 Alexander Egorov

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
    internal class VersionsReader : IVersionsReader
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
                releases.ContinueWith(this.OnError, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        private void OnReleasesListCompleted(Task<IReadOnlyList<Release>> task)
        {
            var lastRelease = task.Result.OrderByDescending(r => r.PublishedAt).FirstOrDefault();
            if (lastRelease == null)
            {
                this.subject.OnCompleted();
                return;
            }

            var assets = this.github.Repository.Release.GetAllAssets(this.account, this.project, lastRelease.Id);
            assets
                    .ContinueWith(this.OnAssetComplete, lastRelease, TaskContinuationOptions.NotOnFaulted)
                    .ContinueWith(t => this.subject.OnCompleted());

            assets.ContinueWith(this.OnError, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void OnAssetComplete(Task<IReadOnlyList<ReleaseAsset>> task, object state)
        {
            var release = (Release)state;
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

        private void OnError(Task task) => this.OnError(task.Exception?.InnerException);

        private void OnError(Exception ex)
        {
            this.subject.OnCompleted();
            Log.Instance.Error(ex?.Message, ex);
        }

        public IDisposable Subscribe(Action<VersionModel> onNext, Action onCompleted) => this.subject.Subscribe(onNext, onCompleted);

        public void Dispose() => this.Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.subject.Dispose();
            }
        }
    }
}
