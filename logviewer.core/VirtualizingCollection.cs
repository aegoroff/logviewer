// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.core
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
    public class VirtualizingCollection<T> : IList<T>, IList, IDisposable
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageTimeoutMilliseconds">The page timeout.</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int pageTimeoutMilliseconds)
        {
            this.ItemsProvider = itemsProvider;
            this.PageSize = pageSize;
            this.PageTimeoutMilliseconds = pageTimeoutMilliseconds;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        /// <param name="pageSize">Size of the page.</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize)
        {
            this.ItemsProvider = itemsProvider;
            this.PageSize = pageSize;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualizingCollection&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="itemsProvider">The items provider.</param>
        public VirtualizingCollection(IItemsProvider<T> itemsProvider)
        {
            this.ItemsProvider = itemsProvider;
        }

        #endregion


        /// <summary>
        ///     Gets the items provider.
        /// </summary>
        /// <value>The items provider.</value>
        public IItemsProvider<T> ItemsProvider { get; }

        /// <summary>
        ///     Gets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize { get; } = 100;

        /// <summary>
        ///     Gets the page timeout.
        /// </summary>
        /// <value>The page timeout.</value>
        public long PageTimeoutMilliseconds { get; } = 10000;

        #region IList<T>, IList

        #region Count

        private const int BadCount = -1;

        private int count = BadCount;

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        ///     The first time this property is accessed, it will fetch the count from the IItemsProvider.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public virtual int Count
        {
            get
            {
                if (this.count == BadCount)
                {
                    this.LoadCount();
                }
                return this.count;
            }
            protected set { this.count = value; }
        }

        #endregion

        #region Indexer

        private T current;

        protected virtual void LoadCurrent(long offset)
        {
            this.current = this.FetchSingle(offset);
        }

        /// <summary>
        ///     Gets the item at the specified index. This property will fetch
        ///     the corresponding page from the IItemsProvider if required.
        /// </summary>
        /// <value></value>
        public T this[int index]
        {
            get
            {
                //this.LoadCurrent(index);
                //return this.current;
                
                // determine which page and offset within page
                var pageIndex = index / this.PageSize;
                var pageOffset = index % this.PageSize;

                // request primary page
                this.RequestPage(pageIndex);

                // if accessing upper 50% then request next page
                if (pageOffset > this.PageSize / 2 && pageIndex < this.Count / this.PageSize)
                {
                    this.RequestPage(pageIndex + 1);
                }

                // if accessing lower 50% then request prev page
                if (pageOffset < this.PageSize / 2 && pageIndex > 0)
                {
                    this.RequestPage(pageIndex - 1);
                }

                // remove stale pages
                this.CleanUpPages();

                // defensive check in case of async load
                IList<T> result;
                if (!this.pages.TryGetValue(pageIndex, out result) || result == null)
                {
                    return default(T);
                }
                // return requested item
                return pageOffset >= result.Count ? default(T) : result[pageOffset];
            }
            set { throw new NotSupportedException(); }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T) value);
        }

        public bool Contains(T item)
        {
            return false;
        }

        /// <summary>
        /// Removes all elements from collection <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public void Clear()
        {
            this.count = BadCount;
            this.pages.Clear();
            this.pageTouchTimes.Clear();
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T) value);
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T) value);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public object SyncRoot => this;
        public bool IsSynchronized => false;
        public bool IsReadOnly => true;

        public bool IsFixedSize => false;

        #endregion

        protected T Current
        {
            get { return this.current; }
            set { this.current = value; }
        }

        #region Paging

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private Dictionary<int, IList<T>> pages = new Dictionary<int, IList<T>>();
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private Dictionary<int, DateTime> pageTouchTimes = new Dictionary<int, DateTime>();

        /// <summary>
        ///     Cleans up any stale pages that have not been accessed in the period dictated by PageTimeoutMilliseconds.
        /// </summary>
        public void CleanUpPages()
        {
            // TODO: performance problem but in other hand you cannot modify collection (this.pageTouchTimes) while iterating
            var keys = new List<int>(this.pageTouchTimes.Keys);

            // Performance reason. Thanks dotTrace from JetBrains :)
            var now = DateTime.Now;
            foreach (var key in keys)
            {
                // page 0 is a special case, since WPF ItemsControl access the first item frequently
                DateTime lastUsed;
                if (key != 0 && this.pageTouchTimes.TryGetValue(key, out lastUsed) && (now - lastUsed).TotalMilliseconds > this.PageTimeoutMilliseconds)
                {
                    this.pages.Remove(key);
                    this.pageTouchTimes.Remove(key);
                    Trace.WriteLine("Removed Page: " + key);
                }
            }
        }

        /// <summary>
        ///     Populates the page within the dictionary.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="page">The page.</param>
        protected virtual void PopulatePage(int pageIndex, IList<T> page)
        {
            Trace.WriteLine("Page populated: " + pageIndex);
            if (this.pages.ContainsKey(pageIndex))
            {
                this.pages[pageIndex] = page;
            }
        }

        /// <summary>
        ///     Makes a request for the specified page, creating the necessary slots in the dictionary,
        ///     and updating the page touch time.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        protected virtual void RequestPage(int pageIndex)
        {
            if (!this.pages.ContainsKey(pageIndex))
            {
                this.pages.Add(pageIndex, null);
                this.pageTouchTimes.Add(pageIndex, DateTime.Now);
                Trace.WriteLine("Added page: " + pageIndex);
                this.LoadPage(pageIndex);
            }
            else
            {
                this.pageTouchTimes[pageIndex] = DateTime.Now;
            }
        }

        #endregion

        #region Load methods

        /// <summary>
        ///     Loads the count of items.
        /// </summary>
        protected virtual void LoadCount()
        {
            this.Count = (int)this.FetchCount();
        }

        /// <summary>
        ///     Loads the page of items.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        protected virtual void LoadPage(int pageIndex)
        {
            this.PopulatePage(pageIndex, this.FetchPage(pageIndex));
        }

        #endregion

        #region Fetch methods

        /// <summary>
        ///     Fetches the requested page from the IItemsProvider.
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        protected IList<T> FetchPage(int pageIndex)
        {
            return this.ItemsProvider.FetchRange(pageIndex * this.PageSize, this.PageSize);
        }

        protected T FetchSingle(long offset)
        {
            return this.ItemsProvider.FetchSingle(offset);
        }

        /// <summary>
        ///     Fetches the count of itmes from the IItemsProvider.
        /// </summary>
        /// <returns></returns>
        protected long FetchCount()
        {
            return this.ItemsProvider.FetchCount();
        }

        #endregion

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~VirtualizingCollection()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}