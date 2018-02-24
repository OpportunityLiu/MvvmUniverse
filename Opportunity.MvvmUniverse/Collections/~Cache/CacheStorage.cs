using Opportunity.Helpers.Universal.AsyncHelpers;
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
    public class CacheStorage<TKey, TCache> : IDictionary<TKey, TCache>
    {
        protected struct CacheDataNode
        {
            public int Prev { get; set; }
            public int Next { get; set; }
            public bool IsEmpty { get; set; }
            public TKey Key { get; set; }
            public TCache Data { get; set; }
        }

        protected CacheDataNode[] CacheData { get; }
        protected Dictionary<TKey, int> CacheIndex { get; }

        public CacheStorage()
            : this(25, null) { }

        public CacheStorage(int capacity)
            : this(capacity, null) { }

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
                this.CacheData[i].Prev = i - 1;
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
                var prevIndex = this.CacheData[index].Prev;
                var nextIndex = this.CacheData[index].Next;
                this.CacheData[prevIndex].Next = nextIndex;
                this.CacheData[nextIndex].Prev = prevIndex;

                var oldFirstIndex = this.CacheData[0].Next;
                this.CacheData[index].Next = oldFirstIndex;
                this.CacheData[oldFirstIndex].Prev = index;
                this.CacheData[index].Prev = 0;
                this.CacheData[0].Next = index;
            }
        }

        private void toLast(int index)
        {
            if (this.CacheData[this.CacheData.Length - 1].Prev != index)
            {
                var prevIndex = this.CacheData[index].Prev;
                var nextIndex = this.CacheData[index].Next;
                this.CacheData[prevIndex].Next = nextIndex;
                this.CacheData[nextIndex].Prev = prevIndex;

                var oldLastIndex = this.CacheData[this.CacheData.Length - 1].Prev;
                this.CacheData[index].Prev = oldLastIndex;
                this.CacheData[oldLastIndex].Next = index;
                this.CacheData[index].Next = this.CacheData.Length - 1;
                this.CacheData[this.CacheData.Length - 1].Prev = index;
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
            var lastIndex = this.CacheData[this.CacheData.Length - 1].Prev;
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

        public int Capacity => this.CacheData.Length - 2;

        public int Count => this.CacheIndex.Count;

        public IEqualityComparer<TKey> Comparer => this.CacheIndex.Comparer;

        public ICollection<TKey> Keys => this.CacheIndex.Keys;

        public ICollection<TCache> Values => throw new NotImplementedException();

        bool ICollection<KeyValuePair<TKey, TCache>>.IsReadOnly => false;

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

        public void Add(TKey key, TCache value) => add(key, value);

        public bool ContainsKey(TKey key) => this.CacheIndex.ContainsKey(key);
        public bool Remove(TKey key)
        {
            if (this.CacheIndex.TryGetValue(key, out var index))
            {
                removeAt(index);
                return true;
            }
            return false;
        }

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

        public void Clear()
        {
            this.CacheIndex.Clear();
            init();
        }

        public IEnumerator<KeyValuePair<TKey, TCache>> GetEnumerator()
        {
            foreach (var item in this.CacheIndex)
            {
                yield return new KeyValuePair<TKey, TCache>(item.Key, this.CacheData[item.Value].Data);
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
