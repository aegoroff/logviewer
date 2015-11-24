// Created by: egr
// Created at: 24.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;

namespace logviewer.core
{
    internal class PagesCache<T> : IDictionary<int, T> where T : class
    {
        private T[] store;

        public PagesCache(int count)
        {
            this.Count = count;
            this.store = new T[count];
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(KeyValuePair<int, T> item)
        {
            this.store[item.Key] = item.Value;
        }

        public void Clear()
        {
            this.store = new T[this.Count];
        }

        public bool Contains(KeyValuePair<int, T> item)
        {
            return this.store[item.Key] != null;
        }

        public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            this.store[item.Key] = null;
            return true;
        }

        public int Count { get; }

        public bool IsReadOnly => false;

        public bool ContainsKey(int key)
        {
            return this.store[key] != null;
        }

        public void Add(int key, T value)
        {
            this.store[key] = value;
        }

        public bool Remove(int key)
        {
            this.store[key] = null;
            return true;
        }

        public bool TryGetValue(int key, out T value)
        {
            value = this.store[key];
            return value != null;
        }

        public T this[int key]
        {
            get { return this.store[key]; }
            set { this.store[key] = value; }
        }

        public ICollection<int> Keys
        {
            get { throw new NotImplementedException(); }
        }

        public ICollection<T> Values => this.store;
    }
}