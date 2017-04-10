// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 24.02.2017
// © 2012-2017 Alexander Egorov

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace logviewer.logic.ui.main
{
    public sealed class UpdatesCheckingModel : UiSynchronizeModel, IDisposable
    {
        private const int CheckUpdatesEveryDays = 7;

        private readonly IScheduler backgroundScheduler;

        private readonly IVersionsReader reader;

        private readonly ISettingsProvider settings;

        private readonly IMainViewModel viewModel;

        public UpdatesCheckingModel(IMainViewModel viewModel, IScheduler backgroundScheduler = null)
            : this(viewModel, new VersionsReader(viewModel.GithubAccount, viewModel.GithubProject), backgroundScheduler)
        {
        }

        public UpdatesCheckingModel(IMainViewModel viewModel, IVersionsReader reader, IScheduler backgroundScheduler = null)
        {
            this.viewModel = viewModel;
            this.settings = viewModel.SettingsProvider;
            this.reader = reader;
            this.backgroundScheduler = backgroundScheduler ?? Scheduler.Default;
        }

        /// <summary>
        ///     Check updates available
        /// </summary>
        /// <param name="manualInvoke">Whether to show no update available in GUI. False by default</param>
        public void CheckUpdates(bool manualInvoke = false)
        {
            if (!manualInvoke)
            {
                if (DateTime.UtcNow < this.settings.LastUpdateCheckTime.AddDays(CheckUpdatesEveryDays))
                {
                    return;
                }
            }

            var checker = new UpdatesChecker(this.reader);

            var observable = Observable.Return(checker, this.backgroundScheduler);

            this.settings.LastUpdateCheckTime = DateTime.UtcNow;

            void OnNext(UpdatesChecker chk)
            {
                void ShowNewVersionAvailable() => this.viewModel.ShowDialogAboutNewVersionAvaliable(chk.CurrentVersion, chk.LatestVersion,
                                                                                                    chk.LatestVersionUrl);

                void ShowNoUpdateAvaliable() => this.viewModel.ShowNoUpdateAvaliable();

                void OnComplete(bool result)
                {
                    if (!result)
                    {
                        if (manualInvoke)
                        {
                            this.RunOnGuiThread(ShowNoUpdateAvaliable);
                        }
                        return;
                    }

                    this.RunOnGuiThread(ShowNewVersionAvailable);
                }

                chk.CheckUpdatesAvaliable(OnComplete);
            }

            observable.Subscribe(OnNext);
        }

        public void Dispose()
        {
            this.reader?.Dispose();
        }
    }
}
