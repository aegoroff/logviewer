// Created by: egr
// Created at: 04.10.2014
// © 2012-2015 Alexander Egorov

using System;
using System.Threading;

namespace logviewer.core
{
    public abstract class BaseGuiController
    {
        private readonly SynchronizationContext winformsOrDefaultContext;

        protected BaseGuiController()
        {
            this.winformsOrDefaultContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public void RunOnGuiThread(Action action)
        {
            this.winformsOrDefaultContext.Post(o => action(), null);
        }
    }
}