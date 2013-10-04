// Created by: egr
// Created at: 01.10.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Threading;

namespace logviewer.core
{
    public class ProducerConsumerQueue
    {
        private readonly object locker = new object();
        private readonly Thread[] workers;
        private readonly Queue<Action> itemQ = new Queue<Action>();

        public ProducerConsumerQueue(int workerCount)
        {
            this.workers = new Thread[workerCount];

            // Create and start a separate thread for each worker
            for (var i = 0; i < workerCount; i++)
            {
                (this.workers[i] = new Thread(this.Consume)).Start();
            }
        }

        public void Shutdown(bool waitForWorkers)
        {
            // Enqueue one null item per worker to make each exit.
            for (var i = 0; i < this.workers.Length; i++)
            {
                this.EnqueueItem(null);
            }

            // Wait for workers to finish
            if (!waitForWorkers)
            {
                return;
            }
            foreach (var worker in this.workers)
            {
                worker.Join();
            }
        }

        public void CleanupPendingTasks()
        {
            lock (this.locker)
            {
                this.itemQ.Clear();
            }
        }

        public int WorkersCount
        {
            get { return this.workers.Length; }
        }
        
        public int Count
        {
            get
            {
                lock (this.locker)
                {
                    return this.itemQ.Count;
                }
            }
        }

        public void EnqueueItem(Action item)
        {
            lock (this.locker)
            {
                this.itemQ.Enqueue(item); // We must pulse because we're
                Monitor.Pulse(this.locker); // changing a blocking condition.
            }
        }

        private void Consume()
        {
            while (true) // Keep consuming until
            {
                // told otherwise.
                Action item;
                lock (this.locker)
                {
                    while (this.itemQ.Count == 0)
                    {
                        Monitor.Wait(this.locker);
                    }
                    item = this.itemQ.Dequeue();
                }
                if (item == null)
                {
                    return; // This signals our exit.
                }
                item(); // Execute item.
            }
        }
    }
}