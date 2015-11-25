// Created by: egr
// Created at: 24.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;

namespace logviewer.core
{
    /// <summary>
    /// This class contains fixed size integer key based dictionary. This implementation is much faster then generic
    /// dictionary but with some limitations.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public unsafe class FixedSizeDictionary<T> : IDictionary<int, T>
    {
        private T[] store;
        private int[] indexes;

        public FixedSizeDictionary(int count)
        {
            this.Count = count;
            this.store = new T[count];
            this.indexes = new int[count];
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            for (var i = 0; i < this.store.Length; i++)
            {
                if (ContainsKey(i))
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
            for (var i = arrayIndex; i < array.Length && i < this.store.Length; i++)
            {
                if (ContainsKey(i))
                {
                    array[i] = new KeyValuePair<int, T>(i, this.store[i]);
                }
            }
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            return this.Remove(item.Key);
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
            this.store[key] = default(T);
            this.indexes[key] = 0;
            return true;
        }

        public bool TryGetValue(int key, out T value)
        {
            fixed (int* p = this.indexes)
            {
                if (p[key] == 0)
                {
                    value = default(T);
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
            get { return this.Select(pair => pair.Key); }
        }

        public ICollection<T> Values
        {
            get { return this.Select(pair => pair.Value); }
        }

        private ICollection<TItem> Select<TItem>(Func<KeyValuePair<int, T>, TItem> selector)
        {
            var result = new List<TItem>();
            var e = this.GetEnumerator();
            using (e)
            {
                while (e.MoveNext())
                {
                    result.Add(selector(e.Current));
                }
                return result;
            }
        }
    }
}