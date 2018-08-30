// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 10.11.2015
// © 2012-2018 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using logviewer.logic.Annotations;
using logviewer.logic.support;
#if DEBUG
using System.Diagnostics;

#endif

namespace logviewer.logic.ui
{
    /// <summary>
    ///     Specialized list implementation that provides data virtualization. The collection is divided up into pages,
    ///     and pages are dynamically fetched from the IItemsProvider when required. Stale pages are removed after a
    ///     configurable period of time.
    ///     Intended for use with large collections on a network or disk resource that cannot be instantiated locally
    ///     due to memory consumption or fetch latency.
    /// </summary>
    /// <remarks>
    ///     The IList implmentation is not fully complete, but should be sufficient for use as read only collection
    ///     data bound to a suitable ItemsControl.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class VirtualizingCollection<T> : IVirtualizingCollection<T>, IList
    {
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize,
                                      int pageCacheTimeoutMilliseconds) : this(itemsProvider, pageSize) =>
                this.pageCacheTimeoutMilliseconds = pageCacheTimeoutMilliseconds;

        [PublicAPI]
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize) : this(itemsProvider) => this.pageSize = pageSize;

        [PublicAPI]
        public VirtualizingCollection(IItemsProvider<T> itemsProvider) => this.ItemsProvider = itemsProvider;

        [PublicAPI]
        public IItemsProvider<T> ItemsProvider { get; }

        [PublicAPI]

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public int PageSize => this.pageSize;

        [PublicAPI]
        public long PageCacheTimeoutMilliseconds => this.pageCacheTimeoutMilliseconds;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => this.CollectionChanged?.Invoke(this, e);

        private void FireCollectionReset()
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);

        private void FirePropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            this.OnPropertyChanged(e);
        }

        private bool isLoading;

        [PublicAPI]
        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                this.isLoading = value;
                this.FirePropertyChanged(nameof(this.IsLoading));
            }
        }

        private void LoadPage(int pageIndex)
        {
            this.IsLoading = true;

            var source = Observable.Create<T[]>(observer =>
            {
                observer.OnNext(this.FetchPage(pageIndex));
                return Disposable.Empty;
            });

            void OnSuccess(T[] result)
            {
                this.PopulatePage(pageIndex, result);
                this.FireCollectionReset();
                this.IsLoading = false;
            }

            void OnError(Exception exception) => this.IsLoading = false;

            source
                    .SubscribeOn(Scheduler.Default)
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(OnSuccess, OnError);
        }

        public void ChangeVisible(Range range)
        {
            this.firstVisible = range.First;
            this.lastVisible = range.Last;
        }

        private int count;

        public int Count
        {
            get => this.count;
            private set
            {
                this.count = value;
                this.pages = new FixedSizeDictionary<T[]>(this.count / this.PageSize + 1);
                this.pageTouchTimes = new FixedSizeDictionary<DateTime>(this.count / this.PageSize + 1);
                this.FireCollectionReset();
            }
        }

        /// <summary>
        ///     Gets the item at the specified index. This property will fetch
        ///     the corresponding page from the IItemsProvider if required.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// this ugly construction only for performance reasons. This indexer may be called hundred million times for
        /// 0.5M items collection.
        /// </remarks>
        public T this[int index]
        {
            get => (T)((IList)this)[index];
            set => throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get
            {
                // determine which page and offset within page
                var pageIndex = index / this.pageSize;
                var pageOffset = index % this.pageSize;

                // do not load invisible. Because WPF try to iterate all collection on Reset event
                if (index >= this.firstVisible && index <= this.lastVisible)
                {
                    // request primary page
                    this.RequestPage(pageIndex);

                    this.CleanUpPages();
                }

                // defensive check in case of async load
                if (this.pages.TryGetValue(pageIndex, out T[] result) && result.Length > 0)
                {
                    return result[pageOffset];
                }

                return default(T);
            }
            set => throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < this.count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(T item) => throw new NotSupportedException();

        int IList.Add(object value) => throw new NotSupportedException();

        bool IList.Contains(object value) => this.Contains((T)value);

        public bool Contains(T item) => false;

        public void Clear()
        {
            this.count = 0;
            this.pages.Clear();
            this.pageTouchTimes.Clear();
        }

        int IList.IndexOf(object value) => this.IndexOf((T)value);

        [Pure]
        public int IndexOf(T item) => -1;

        public void Insert(int index, T item) => throw new NotSupportedException();

        void IList.Insert(int index, object value) => this.Insert(index, (T)value);

        public void RemoveAt(int index) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        public bool Remove(T item) => throw new NotSupportedException();

        public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();

        void ICollection.CopyTo(Array array, int index) => throw new NotSupportedException();

        public object SyncRoot => this;

        public bool IsSynchronized => false;

        public bool IsReadOnly => true;

        public bool IsFixedSize => false;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private FixedSizeDictionary<T[]> pages = new FixedSizeDictionary<T[]>(1);

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private FixedSizeDictionary<DateTime> pageTouchTimes = new FixedSizeDictionary<DateTime>(1);

        private readonly int pageSize = 100;

        private int firstVisible;

        private int lastVisible;

        private readonly long pageCacheTimeoutMilliseconds = 10000;

        /// <summary>
        ///     Cleans up any stale pages that have not been accessed in the period dictated by PageCacheTimeoutMilliseconds.
        /// </summary>
        [PublicAPI]
        public void CleanUpPages()
        {
            var keys = (List<int>)this.pageTouchTimes.Keys;

            // Performance reason. Thanks dotTrace from JetBrains :)
            var now = DateTime.Now;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];

                // page 0 is a special case, since WPF ItemsControl access the first item frequently
                if (key != 0
                    && this.pageTouchTimes.TryGetValue(key, out DateTime lastUsed)
                    && (now - lastUsed).TotalMilliseconds > this.pageCacheTimeoutMilliseconds)
                {
                    this.pages.Remove(key);
                    this.pageTouchTimes.Remove(key);
#if DEBUG
                    Trace.WriteLine("Removed Page: " + key);
#endif
                }
            }
        }

        /// <summary>
        ///     Populates the page within the dictionary.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="page">The page.</param>
        private void PopulatePage(int pageIndex, T[] page)
        {
#if DEBUG
            Trace.WriteLine("Page populated: " + pageIndex);
#endif
            if (this.pages.ContainsKey(pageIndex))
            {
                this.pages[pageIndex] = page;
            }
        }

        /// <summary>
        ///     Makes a request for the specified page, creating the necessary slots in the dictionary,
        ///     and updating the page touch time.
        /// </summary>
        private void RequestPage(int pageIndex)
        {
            if (!this.pageTouchTimes.ContainsKey(pageIndex))
            {
                this.pages.Add(pageIndex, new T[0]);
                this.pageTouchTimes.Add(pageIndex, DateTime.Now);
#if DEBUG
                Trace.WriteLine("Added page: " + pageIndex);
#endif
                this.LoadPage(pageIndex);
            }
            else
            {
                this.pageTouchTimes[pageIndex] = DateTime.Now;
            }
        }

        /// <summary>
        ///     Loads the count of items.
        /// </summary>
        public void LoadCount(int itemsCount) => this.Count = itemsCount;

        private T[] FetchPage(int pageIndex) => this.ItemsProvider.FetchRange(pageIndex * this.PageSize, this.PageSize);
    }
}
