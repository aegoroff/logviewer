// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 26.04.2017
// © 2012-2017 Alexander Egorov

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace logviewer.engine.strings
{
    /// <summary>
    ///     This class contains fixed size positive char key based dictionary. This implementation is much faster then generic
    ///     dictionary but with some limitations.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public sealed class CharKeyDictionary<T> : IDictionary<char, T>
    {
        private int[] indexes;

        private T[] store;

        private List<char> keys;

        /// <summary>
        /// Ininitalizes new dictionary instance
        /// </summary>
        public CharKeyDictionary() => this.Clear();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<char, T>> GetEnumerator()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < this.keys.Count; i++)
            {
                var ix = this.keys[i];
                yield return new KeyValuePair<char, T>(ix, this.store[ix]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc />
        public void Add(KeyValuePair<char, T> item) => this.Add(item.Key, item.Value);

        /// <inheritdoc />
        public void Clear()
        {
            this.store = new T[char.MaxValue];
            this.indexes = new int[char.MaxValue];
            this.keys = new List<char>(char.MaxValue);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<char, T> item) => this.TryGetValue(item.Key, out T value) && Equals(value, item.Value);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<char, T>[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length && i < this.store.Length; i++)
            {
                if (this.ContainsKey((char)i))
                {
                    array[i] = new KeyValuePair<char, T>((char)i, this.store[i]);
                }
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<char, T> item) => this.Remove(item.Key);

        /// <inheritdoc />
        public int Count => this.keys.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool ContainsKey(char key) => this.indexes[key] > 0;

        /// <inheritdoc />
        public void Add(char key, T value)
        {
            if (this.ContainsKey(key))
            {
                return;
            }

            this.store[key] = value;
            this.indexes[key] = 1;
            this.keys.Add(key);
        }

        /// <inheritdoc />
        public bool Remove(char key)
        {
            if (!this.ContainsKey(key))
            {
                return false;
            }

            this.store[key] = default(T);
            this.indexes[key] = 0;
            this.keys.Remove(key);
            return true;
        }

        /// <inheritdoc />
        public bool TryGetValue(char key, out T value)
        {
            if (this.indexes[key] == 0)
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
        public T this[char key]
        {
            get => this.store[key];
            set => this.store[key] = value;
        }

        /// <inheritdoc />
        public ICollection<char> Keys => this.keys;

        /// <inheritdoc />
        public ICollection<T> Values => this.GetValuesInternal().ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<T> GetValuesInternal()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < this.keys.Count; i++)
            {
                yield return this.store[this.keys[i]];
            }
        }
    }
}
