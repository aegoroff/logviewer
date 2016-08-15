// Created by: egr
// Created at: 04.10.2014
// © 2012-2016 Alexander Egorov

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