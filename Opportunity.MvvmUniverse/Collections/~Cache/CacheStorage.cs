using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Collection of cached items.
    /// </summary>
    /// <typeparam name="TKey">Key type of cache.</typeparam>
    /// <typeparam name="TCache">Value type of cache.</typeparam>
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public class CacheStorage<TKey, TCache> : IDictionary<TKey, TCache>
    {
        /// <summary>
        /// Stored cache data.
        /// </summary>
        protected struct CacheDataNode
        {
            /// <summary>
            /// Index of previous node.
            /// </summary>
            public int Previous { get; set; }
            /// <summary>
            /// Index of next node.
            /// </summary>
            public int Next { get; set; }
            /// <summary>
            /// Indicates current node is empty or not.
            /// </summary>
            public bool IsEmpty { get; set; }
            /// <summary>
            /// Key of current node.
            /// </summary>
            public TKey Key { get; set; }
            /// <summary>
            /// Value of current node.
            /// </summary>
            public TCache Data { get; set; }
        }

        /// <summary>
        /// Storage of cache nodes, has a length of (<see cref="Capacity"/> + 2).
        /// </summary>
        protected CacheDataNode[] CacheData { get; }
        /// <summary>
        /// Index of cache nodes, maps key to index of <see cref="CacheData"/>.
        /// </summary>
        protected Dictionary<TKey, int> CacheIndex { get; }

        /// <summary>
        /// Create new instance of <see cref="CacheStorage{TKey, TCache}"/>.
        /// </summary>
        public CacheStorage()
            : this(25, null) { }

        /// <summary>
        /// Create new instance of <see cref="CacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <param name="capacity">Capacity of stroage.</param>
        public CacheStorage(int capacity)
            : this(capacity, null) { }

        /// <summary>
        /// Create new instance of <see cref="CacheStorage{TKey, TCache}"/>.
        /// </summary>
        /// <param name="capacity">Capacity of stroage.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> for key comparsion.</param>
        public CacheStorage(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.CacheIndex = new Dictionary<TKey, int>(capacity, comparer);
            this.CacheData = new CacheDataNode[capacity + 2];
            init();
        }

        private void init()
        {
            for (var i = 0; i < this.CacheData.Length; i++)
            {
                this.CacheData[i].Previous = i - 1;
                this.CacheData[i].Next = i + 1;
                this.CacheData[i].Key = default;
                this.CacheData[i].Data = default;
                this.CacheData[i].IsEmpty = true;
            }
        }

        private void toFirst(int index)
        {
            if (this.CacheData[0].Next != index)
            {
                var prevIndex = this.CacheData[index].Previous;
                var nextIndex = this.CacheData[index].Next;
                this.CacheData[prevIndex].Next = nextIndex;
                this.CacheData[nextIndex].Previous = prevIndex;

                var oldFirstIndex = this.CacheData[0].Next;
                this.CacheData[index].Next = oldFirstIndex;
                this.CacheData[oldFirstIndex].Previous = index;
                this.CacheData[index].Previous = 0;
                this.CacheData[0].Next = index;
            }
        }

        private void toLast(int index)
        {
            if (this.CacheData[this.CacheData.Length - 1].Previous != index)
            {
                var prevIndex = this.CacheData[index].Previous;
                var nextIndex = this.CacheData[index].Next;
                this.CacheData[prevIndex].Next = nextIndex;
                this.CacheData[nextIndex].Previous = prevIndex;

                var oldLastIndex = this.CacheData[this.CacheData.Length - 1].Previous;
                this.CacheData[index].Previous = oldLastIndex;
                this.CacheData[oldLastIndex].Next = index;
                this.CacheData[index].Next = this.CacheData.Length - 1;
                this.CacheData[this.CacheData.Length - 1].Previous = index;
            }
        }

        private TCache getAt(int index)
        {
            toFirst(index);
            Debug.Assert(this.CacheData[index].IsEmpty == false);
            return this.CacheData[index].Data;
        }

        private void setAt(int index, TKey key, TCache value)
        {
            toFirst(index);
            Debug.Assert(this.CacheData[index].IsEmpty == false);
            this.CacheData[index].Key = key;
            this.CacheData[index].Data = value;
        }

        private void add(TKey key, TCache value)
        {
            this.CacheIndex.Add(key, -1);
            var lastIndex = this.CacheData[this.CacheData.Length - 1].Previous;
            if (!this.CacheData[lastIndex].IsEmpty)
            {
                var oldKey = this.CacheData[lastIndex].Key;
                this.CacheIndex.Remove(oldKey);
            }
            this.CacheData[lastIndex].IsEmpty = false;
            setAt(lastIndex, key, value);
            this.CacheIndex[key] = lastIndex;
            toFirst(lastIndex);
        }

        private void removeAt(int index)
        {
            toLast(index);
            this.CacheIndex.Remove(this.CacheData[index].Key);
            Debug.Assert(this.CacheData[index].IsEmpty == false);
            this.CacheData[index].Key = default;
            this.CacheData[index].Data = default;
            this.CacheData[index].IsEmpty = true;
        }

        /// <summary>
        /// The capacity of cache.
        /// </summary>
        public int Capacity => this.CacheData.Length - 2;

        /// <summary>
        /// Count of cached items.
        /// </summary>
        public int Count => this.CacheIndex.Count;

        /// <summary>
        /// <see cref="IEqualityComparer{T}"/> for key comparsion.
        /// </summary>
        public IEqualityComparer<TKey> Comparer => this.CacheIndex.Comparer;

        /// <summary>
        /// Keys of cached items.
        /// </summary>
        public ICollection<TKey> Keys => this.CacheIndex.Keys;

        /// <exception cref="NotImplementedException">Not implemented.</exception>
        public ICollection<TCache> Values => throw new NotImplementedException();

        bool ICollection<KeyValuePair<TKey, TCache>>.IsReadOnly => false;

        /// <summary>
        /// Get cached item.
        /// </summary>
        /// <param name="key">Key of item.</param>
        /// <returns>Cached item.</returns>
        /// <exception cref="KeyNotFoundException">Key not found in the cache storage.</exception>
        public TCache this[TKey key]
        {
            get => getAt(this.CacheIndex[key]);
            set
            {
                if (this.CacheIndex.TryGetValue(key, out var index))
                    setAt(index, key, value);
                else
                    add(key, value);
            }
        }

        /// <summary>
        /// Add cached item to storage.
        /// </summary>
        /// <param name="key">Key of item.</param>
        /// <param name="value">Cached item.</param>
        /// <exception cref="ArgumentException">Cache storage has item with same key.</exception>
        public void Add(TKey key, TCache value) => add(key, value);

        /// <summary>
        /// Check key in cache storage.
        /// </summary>
        /// <param name="key">Key of item.</param>
        /// <returns><see langword="true"/> if key in the storage.</returns>
        public bool ContainsKey(TKey key) => this.CacheIndex.ContainsKey(key);

        /// <summary>
        /// Remove cached item from storage.
        /// </summary>
        /// <param name="key">Key of item.</param>
        /// <returns><see langword="true"/> if key in the storage and its value is removed.</returns>
        public bool Remove(TKey key)
        {
            if (this.CacheIndex.TryGetValue(key, out var index))
            {
                removeAt(index);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Try get cached item.
        /// </summary>
        /// <param name="key">Key of item.</param>
        /// <param name="value">Cached item.</param>
        /// <returns><see langword="true"/> if key in the storage.</returns>
        public bool TryGetValue(TKey key, out TCache value)
        {
            if (this.CacheIndex.TryGetValue(key, out var index))
            {
                value = getAt(index);
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Clear the storage.
        /// </summary>
        public void Clear()
        {
            this.CacheIndex.Clear();
            init();
        }

        /// <summary>
        /// Get enumerator.
        /// </summary>
        /// <returns>Enumerator of current storage.</returns>
        public IEnumerator<KeyValuePair<TKey, TCache>> GetEnumerator()
        {
            for (var i = 1; i < this.CacheData.Length - 1; i++)
            {
                var item = this.CacheData[i];
                if (item.IsEmpty)
                    break;
                yield return new KeyValuePair<TKey, TCache>(item.Key, item.Data);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<KeyValuePair<TKey, TCache>>.Add(KeyValuePair<TKey, TCache> item) => Add(item.Key, item.Value);
        bool ICollection<KeyValuePair<TKey, TCache>>.Contains(KeyValuePair<TKey, TCache> item)
        {
            if (this.CacheIndex.TryGetValue(item.Key, out var index))
            {
                var value = this.CacheData[index].Data;
                return EqualityComparer<TCache>.Default.Equals(value, item.Value);
            }
            return false;
        }
        void ICollection<KeyValuePair<TKey, TCache>>.CopyTo(KeyValuePair<TKey, TCache>[] array, int arrayIndex)
        {
            foreach (var item in this.CacheIndex)
            {
                if (arrayIndex >= array.Length)
                    break;
                array[arrayIndex] = new KeyValuePair<TKey, TCache>(item.Key, this.CacheData[item.Value].Data);
                arrayIndex++;
            }
        }
        bool ICollection<KeyValuePair<TKey, TCache>>.Remove(KeyValuePair<TKey, TCache> item)
        {
            if (this.CacheIndex.TryGetValue(item.Key, out var index))
            {
                var value = this.CacheData[index].Data;
                if (EqualityComparer<TCache>.Default.Equals(value, item.Value))
                {
                    return Remove(item.Key);
                }
            }
            return false;
        }
    }

}
