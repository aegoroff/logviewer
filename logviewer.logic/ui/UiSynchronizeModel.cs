// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.10.2014
// © 2012-2017 Alexander Egorov

using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using logviewer.logic.support;

namespace logviewer.logic.ui
{
    public abstract class UiSynchronizeModel
    {
        private readonly TaskScheduler uiSyncContext;

        private readonly SynchronizationContext winformsOrDefaultContext;

        protected UiSynchronizeModel()
        {
            this.winformsOrDefaultContext = SynchronizationContext.Current ?? new SynchronizationContext();
            try
            {
                this.uiSyncContext = TaskScheduler.FromCurrentSynchronizationContext();
            }
            catch (InvalidOperationException e)
            {
                // in some cases TaskScheduler.FromCurrentSynchronizationContext(); may fail for example on running under R# to calculate coverage
                Log.Instance.Error(e.Message, e);
                this.uiSyncContext = TaskScheduler.Current;
            }
            this.UiContextScheduler = new SynchronizationContextScheduler(this.winformsOrDefaultContext);
        }

        protected SynchronizationContextScheduler UiContextScheduler { get; }

        protected void RunOnGuiThread(Action action) => this.winformsOrDefaultContext.Post(o => action(), null);

        protected void CompleteTask(Task task, TaskContinuationOptions options, Action<Task> action)
        {
            void ContinuationAction(Task obj)
            {
                action(task);
                task.Dispose();
            }

            task.ContinueWith(ContinuationAction, CancellationToken.None, options, this.uiSyncContext);
        }
    }
}
