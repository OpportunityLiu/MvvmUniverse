using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class Mscorlib_DictionaryDebugView<K, V>
    {
        private IDictionary<K, V> dict;

        public Mscorlib_DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            this.dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                KeyValuePair<K, V>[] items = new KeyValuePair<K, V>[dict.Count];
                dict.CopyTo(items, 0);
                return items;
            }
        }
    }

    [DebuggerTypeProxy(typeof(Mscorlib_DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class ObservableDictionary<TKey, TValue> : ObservableCollectionBase, IDictionary<TKey, TValue>, IDictionary, IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
    {
        protected Dictionary<TKey, TValue> Items { get; }
        protected List<TKey> SortedKeys { get; } = new List<TKey>();

        public IEqualityComparer<TKey> Comparer => Items.Comparer;

        public ObservableDictionary() : this(EqualityComparer<TKey>.Default) { }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            this.Items = new Dictionary<TKey, TValue>(comparer);
        }

        protected virtual void InsertItem(TKey key, TValue value)
        {
            Items.Add(key, value);
            SortedKeys.Add(key);
            RaiseCollectionAdd(new KeyValuePair<TKey, TValue>(key, value), SortedKeys.Count - 1);
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void RemoveItem(TKey key)
        {
            var removedValue = Items[key];
            Items.Remove(key);
            var index = indexOf(key);
            SortedKeys.Remove(key);
            RaiseCollectionRemove(new KeyValuePair<TKey, TValue>(key, removedValue), index);
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void SetItem(TKey key, TValue value)
        {
            var oldV = Items[key];
            Items[key] = value;
            var index = indexOf(key);
            RaiseCollectionReplace(new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, oldV), index);
        }

        protected virtual void ClearItems()
        {
            Items.Clear();
            SortedKeys.Clear();
            RaiseCollectionReset();
            RaisePropertyChanged(nameof(Count));
        }

        private int indexOf(TKey key)
        {
            return SortedKeys.FindLastIndex(SortedKeys.Count - 1, SortedKeys.Count, v => Comparer.Equals(v, key));
        }

        private void insert(TKey key, TValue value)
        {
            if (Items.ContainsKey(key))
                SetItem(key, value);
            else
                InsertItem(key, value);
        }

        private static TValue castValue(object value)
        {
            if (value == null && default(TValue) == null)
                return default(TValue);
            if (value is TValue v)
                return v;
            throw new ArgumentException("Wrong type of value", nameof(value));
        }

        private static TKey castKey(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key is TKey v)
                return v;
            throw new ArgumentException("Wrong type of key", nameof(key));
        }

        private static KeyValuePair<TKey, TValue> castKV(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is KeyValuePair<TKey, TValue> v)
                return v;
            throw new ArgumentException("Wrong type of value", nameof(value));
        }

        public TValue this[TKey key]
        {
            get => Items[key];
            set => insert(key, value);
        }
        object IDictionary.this[object key]
        {
            get => Items[castKey(key)];
            set => insert(castKey(key), castValue(value));
        }
        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get
            {
                var key = SortedKeys[index];
                return new KeyValuePair<TKey, TValue>(key, Items[key]);
            }
            set
            {
                var oldKey = SortedKeys[index];
                if (Comparer.Equals(oldKey, value.Key))
                    this[oldKey] = value.Value;
                else
                {
                    RemoveItem(oldKey);
                    insert(value.Key, value.Value);
                }
            }
        }
        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
        object IList.this[int index]
        {
            get => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
            set => ((IList<KeyValuePair<TKey, TValue>>)this)[index] = castKV(value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ObservableKeyCollection keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ObservableValueCollection values;

        public ObservableKeyCollection Keys
            => System.Threading.LazyInitializer.EnsureInitialized(ref this.keys, () => new ObservableKeyCollection(this));
        public ObservableValueCollection Values
            => System.Threading.LazyInitializer.EnsureInitialized(ref this.values, () => new ObservableValueCollection(this));

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Keys => Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Values => Values;

        public int Count => SortedKeys.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsFixedSize => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

        public void Add(TKey key, TValue value) => InsertItem(key, value);

        public bool ContainsKey(TKey key) => Items.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (!Items.ContainsKey(key))
                return false;
            RemoveItem(key);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value) => Items.TryGetValue(key, out value);

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => insert(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Items).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Items).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Items.TryGetValue(item.Key, out var val))
                return false;
            if (!EqualityComparer<TValue>.Default.Equals(val, item.Value))
                return false;
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in SortedKeys)
            {
                yield return new KeyValuePair<TKey, TValue>(item, Items[item]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IDictionary.Add(object key, object value) => Add(castKey(key), castValue(value));

        bool IDictionary.Contains(object key) => ContainsKey(castKey(key));

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly ObservableDictionary<TKey, TValue> parents;

            private readonly List<TKey>.Enumerator keyEmulator;

            internal DictionaryEnumerator(ObservableDictionary<TKey, TValue> parents)
            {
                this.parents = parents;
                this.keyEmulator = parents.SortedKeys.GetEnumerator();
            }

            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            public object Key => this.keyEmulator.Current;

            public object Value => this.parents[this.keyEmulator.Current];

            public object Current => new KeyValuePair<TKey, TValue>(this.keyEmulator.Current, this.parents[this.keyEmulator.Current]);

            public bool MoveNext() => this.keyEmulator.MoveNext();

            public void Reset() => ((IEnumerator)this.keyEmulator).Reset();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(this);

        void IDictionary.Remove(object key) => Remove(castKey(key));

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            var index = indexOf(item.Key);
            if (index == -1)
                return -1;
            var k = SortedKeys[index];
            var v = Items[k];
            if (EqualityComparer<TValue>.Default.Equals(v, item.Value))
                return index;
            return -1;
        }

        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
            => insert(item.Key, item.Value);

        void IList<KeyValuePair<TKey, TValue>>.RemoveAt(int index)
        {
            var k = SortedKeys[index];
            RemoveItem(k);
        }

        int IList.Add(object value)
        {
            ((IList<KeyValuePair<TKey, TValue>>)this).Add(castKV(value));
            return this.Count - 1;
        }

        bool IList.Contains(object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Contains(castKV(value));

        int IList.IndexOf(object value) => ((IList<KeyValuePair<TKey, TValue>>)this).IndexOf(castKV(value));

        void IList.Insert(int index, object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Insert(index, castKV(value));

        void IList.Remove(object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Remove(castKV(value));

        void IList.RemoveAt(int index) => ((IList<KeyValuePair<TKey, TValue>>)this).RemoveAt(index);

        public void Clear() => ClearItems();
    }
}
