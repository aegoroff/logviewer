// Created by: egr
// Created at: 04.03.2016
// © 2012-2016 Alexander Egorov

using System.Threading;
using logviewer.engine;
using logviewer.logic.storage;

namespace logviewer.logic.support
{
    internal class ProducerConsumerMessageQueue : ProducerConsumerQueue<LogMessage>
    {
        private LogStore store;

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

        internal void IncrementQueuedCount()
        {
            Interlocked.Increment(ref this.queuedMessages);
        }

        internal void ResetQueuedCount()
        {
            this.queuedMessages = 0;
        }

        internal bool ReadCompleted => Interlocked.Read(ref this.queuedMessages) == 0;

        internal LogStore Store
        {
            get { return this.store; }
            set { this.store = value; }
        }

        internal long QueuedMessages => Interlocked.Read(ref this.queuedMessages);
    }
}