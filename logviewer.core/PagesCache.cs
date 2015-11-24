// Created by: egr
// Created at: 24.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace logviewer.core
{
    internal unsafe class PagesCache<T> : IDictionary<int, T> where T : class
    {
        private T[] store;
        private int[] indexes;

        public PagesCache(int count)
        {
            this.Count = count;
            this.store = new T[count];
            this.indexes = new int[count];
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < this.store.Length; i++)
            {
                if (this.store[i] != null)
                {
                    yield return new KeyValuePair<int, T>(i, this.store[i]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<int, T> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.store = new T[this.Count];
            this.indexes = new int[this.Count];
        }

        public bool Contains(KeyValuePair<int, T> item)
        {
            return this.indexes[item.Key] > 0;
        }

        public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            this.store[item.Key] = null;
            this.indexes[item.Key] = 0;
            return true;
        }

        public int Count { get; }

        public bool IsReadOnly => false;

        public bool ContainsKey(int key)
        {
            fixed (int* p = this.indexes)
            {
                return p[key] > 0;
            }
        }

        public void Add(int key, T value)
        {
            this.store[key] = value;
            this.indexes[key] = 1;
        }

        public bool Remove(int key)
        {
            this.store[key] = null;
            return true;
        }

        public bool TryGetValue(int key, out T value)
        {
            fixed (int* p = this.indexes)
            {
                if (p[key] == 0)
                {
                    value = null;
                    return false;
                }
            }
            value = this.store[key];
            return true;
        }

        public T this[int key]
        {
            get { return this.store[key]; }
            set { this.store[key] = value; }
        }

        public ICollection<int> Keys
        {
            get
            {
                var ix = 0;
                return this.store.Where(v => v != null).Select(v => ix++).ToArray();
            }
        }

        public ICollection<T> Values => this.store.Where(v=> v != null).ToArray();
    }
}