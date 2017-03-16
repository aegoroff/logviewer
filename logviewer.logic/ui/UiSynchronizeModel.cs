// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.10.2014
// © 2012-2017 Alexander Egorov

using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace logviewer.logic.ui
{
    public abstract class UiSynchronizeModel
    {
        private readonly TaskScheduler uiSyncContext;
        private readonly SynchronizationContext winformsOrDefaultContext;

        protected UiSynchronizeModel()
        {
            this.winformsOrDefaultContext = SynchronizationContext.Current ?? new SynchronizationContext();
            this.uiSyncContext = TaskScheduler.FromCurrentSynchronizationContext();
            this.UiContextScheduler = new SynchronizationContextScheduler(this.winformsOrDefaultContext);
        }

        protected SynchronizationContextScheduler UiContextScheduler { get; }

        protected void RunOnGuiThread(Action action)
        {
            this.winformsOrDefaultContext.Post(o => action(), null);
        }

        protected void CompleteTask(Task task, TaskContinuationOptions options, Action<Task> action)
        {
            task.ContinueWith(delegate
            {
                action(task);
                task.Dispose();
            }, CancellationToken.None, options, this.uiSyncContext);
        }
    }
}