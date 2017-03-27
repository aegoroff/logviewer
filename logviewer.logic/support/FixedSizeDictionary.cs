// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 24.11.2015
// © 2012-2017 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using logviewer.logic.Annotations;

namespace logviewer.logic.support
{
    /// <summary>
    ///     This class contains fixed size positive integer key based dictionary. This implementation is much faster then generic
    ///     dictionary but with some limitations.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">Occurs if count is negative o zero</exception>
    public class FixedSizeDictionary<T> : IDictionary<int, T>
    {
        private readonly int count;
        private int[] indexes;
        private T[] store;

        public FixedSizeDictionary(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            this.count = count;
            this.store = new T[count];
            this.indexes = new int[count];
        }

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            for (var i = 0; i < this.store.Length; i++)
            {
                if (this.ContainsKeyInternal(i))
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
            this.store = new T[this.count];
            this.indexes = new int[this.count];
        }

        [Pure]
        public bool Contains(KeyValuePair<int, T> item)
        {
            return this.ContainsKey(item.Key) && Equals(this.store[item.Key], item.Value);
        }

        public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length && i < this.store.Length; i++)
            {
                if (this.ContainsKeyInternal(i))
                {
                    array[i] = new KeyValuePair<int, T>(i, this.store[i]);
                }
            }
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            return this.Remove(item.Key);
        }

        public int Count
        {
            get
            {
                var result = 0;
                for (var i = 0; i < this.store.Length; i++)
                {
                    if (this.ContainsKeyInternal(i))
                    {
                        result++;
                    }
                }
                return result;
            }
        }

        public bool IsReadOnly => false;

        [Pure]
        public bool ContainsKey(int key)
        {
            return key < this.count && key >= 0 &&  this.ContainsKeyInternal(key);
        }

        /// <inheritdoc />
        public void Add(int key, T value)
        {
            if (key >= this.count || key < 0)
            {
                return;
            }
            this.store[key] = value;
            this.indexes[key] = 1;
        }

        /// <inheritdoc />
        public bool Remove(int key)
        {
            if (key >= this.count || key < 0 || !this.ContainsKeyInternal(key))
            {
                return false;
            }
            this.store[key] = default(T);
            this.indexes[key] = 0;
            return true;
        }

        /// <inheritdoc />
        [Pure]
        public bool TryGetValue(int key, out T value)
        {
            if (key >= this.count || key < 0 || this.indexes[key] == 0)
            {
                value = default(T);
                return false;
            }
            value = this.store[key];
            return true;
        }

        /// <summary>
        /// Returns value by key specified.
        /// </summary>
        /// <param name="key">Key to get value of</param>
        /// <exception cref="IndexOutOfRangeException">Occurs in case of key is less then zero or greater then the collection max size</exception>
        public T this[int key]
        {
            get { return this.store[key]; }
            set { this.store[key] = value; }
        }

        public ICollection<int> Keys => this.GetKeysInternal().ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<int> GetKeysInternal()
        {
            for (var i = 0; i < this.indexes.Length; i++)
            {
                if (this.ContainsKeyInternal(i))
                {
                    yield return i;
                }
            }
        }

        public ICollection<T> Values => this.GetValuesInternal().ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<T> GetValuesInternal()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < this.store.Length; i++)
            {
                if (this.ContainsKeyInternal(i))
                {
                    yield return this.store[i];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ContainsKeyInternal(int key)
        {
            return this.indexes[key] > 0;
        }
    }
}