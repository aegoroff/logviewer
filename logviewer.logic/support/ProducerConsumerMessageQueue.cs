// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 04.03.2016
// © 2012-2017 Alexander Egorov

using System.Threading;
using logviewer.engine;
using logviewer.logic.storage;

namespace logviewer.logic.support
{
    internal class ProducerConsumerMessageQueue : ProducerConsumerQueue<LogMessage>
    {
        private ILogStore store;

        private long queuedMessages;

        internal ProducerConsumerMessageQueue(int workerCount) : base(workerCount)
        {
        }

        protected override void Handler(LogMessage item)
        {
            try
            {
                this.store.AddMessage(item);
            }
            finally
            {
                // Interlocked is a must because other threads can change this
                Interlocked.Decrement(ref this.queuedMessages);
            }
        }

        public override void EnqueueItem(LogMessage item)
        {
            Interlocked.Increment(ref this.queuedMessages);
            base.EnqueueItem(item);
        }

        internal void ResetQueuedCount() => this.queuedMessages = 0;

        internal bool ReadCompleted => Interlocked.Read(ref this.queuedMessages) == 0;

        internal void SetStore(ILogStore value) => this.store = value;

        internal long QueuedMessages => Interlocked.Read(ref this.queuedMessages);
    }
}
