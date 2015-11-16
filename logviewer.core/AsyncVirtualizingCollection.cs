// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace logviewer.core
{
    /// <summary>
    /// Derived VirtualizatingCollection, performing loading asychronously.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    public class AsyncVirtualizingCollection<T> : VirtualizingCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider)
            : base(itemsProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
            : base(itemsProvider, pageSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncVirtualizingCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeoutMilliseconds">The page timeout.</param>
        public AsyncVirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeoutMilliseconds)
            : base(itemsProvider, pageSize, pageTimeoutMilliseconds)
        {
        }

        #endregion

        private readonly TaskScheduler uiSyncContext = TaskScheduler.FromCurrentSynchronizationContext();

        #region INotifyCollectionChanged

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Fires the collection reset event.
        /// </summary>
        private void FireCollectionReset()
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(e);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void FirePropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            this.OnPropertyChanged(e);
        }

        #endregion

        #region IsLoading

        private bool isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether the collection is loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this collection is loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }
            set
            {
                this.isLoading = value;
                this.FirePropertyChanged(nameof(this.IsLoading));
            }
        }

        #endregion

        #region Load overrides

        /// <summary>
        /// Asynchronously loads the count of items.
        /// </summary>
        protected override void LoadCount()
        {
            this.Count = 0;
            this.IsLoading = true;

            Task<long>.Factory.StartNew(this.FetchCount).ContinueWith(delegate (Task<long> t)
            {
                this.Count = (int)t.Result;
                this.IsLoading = false;
                this.FireCollectionReset();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.uiSyncContext);
        }

        /// <summary>
        /// Asynchronously loads the page.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void LoadPage(int index)
        {
            this.IsLoading = true;
            var task = Task<IList<T>>.Factory.StartNew(() => this.FetchPage(index));

            task.ContinueWith(delegate (Task<IList<T>> t)
            {
                this.PopulatePage(index, t.Result);
                this.IsLoading = false;
                this.FireCollectionReset();
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.uiSyncContext);
        }

        #endregion
    }
}
