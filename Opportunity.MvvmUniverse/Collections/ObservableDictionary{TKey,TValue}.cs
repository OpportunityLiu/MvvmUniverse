using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Collections
{
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class ObservableDictionary<TKey, TValue> : ObservableCollectionBase
        , IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary
        , IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
    {
        protected List<KeyValuePair<TKey, TValue>> Items { get; } = new List<KeyValuePair<TKey, TValue>>();
        protected Dictionary<TKey, int> KeySet { get; }

        public IEqualityComparer<TKey> Comparer => KeySet.Comparer;

        public ObservableDictionary() : this(EqualityComparer<TKey>.Default) { }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            this.KeySet = new Dictionary<TKey, int>(comparer);
        }

        private void updateIndex(int startIndex, int length)
        {
            for (var i = 0; i < length; i++)
            {
                var item = Items[startIndex];
                KeySet[item.Key] = startIndex;
                startIndex++;
            }
        }

        protected virtual void InsertItem(TKey key, TValue value, int index)
        {
            if (index < 0 || index > KeySet.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            KeySet.Add(key, index);
            var toInsert = Helpers.CreateKVP(key, value);
            Items.Add(toInsert);
            updateIndex(index + 1, Items.Count - index - 1);
            if (this.keys != null)
            {
                this.keys.RaiseCountChangedInternal();
                this.keys.RaiseCollectionAddInternal(toInsert.Key, Items.Count - 1);
            }
            if (this.values != null)
            {
                this.values.RaiseCountChangedInternal();
                this.values.RaiseCollectionAddInternal(toInsert.Value, Items.Count - 1);
            }
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionAdd(toInsert, Items.Count - 1);
        }

        protected virtual void RemoveItem(TKey key)
        {
            var removedIndex = KeySet[key];
            var removedValue = Items[removedIndex];
            Items.RemoveAt(removedIndex);
            updateIndex(removedIndex, Items.Count - removedIndex);
            if (this.keys != null)
            {
                this.keys.RaiseCountChangedInternal();
                this.keys.RaiseCollectionRemoveInternal(removedValue.Key, removedIndex);
            }
            if (this.values != null)
            {
                this.values.RaiseCountChangedInternal();
                this.values.RaiseCollectionRemoveInternal(removedValue.Value, removedIndex);
            }
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionRemove(removedValue, removedIndex);
        }

        protected virtual void SetItem(TKey key, TValue value)
        {
            var index = KeySet[key];
            var oldValue = Items[index];
            var newValue = Helpers.CreateKVP(key, value);
            Items[index] = newValue;
            if (this.keys != null)
            {
                this.keys.RaiseCollectionReplaceInternal(newValue.Key, oldValue.Key, index);
            }
            if (this.values != null)
            {
                this.values.RaiseCollectionReplaceInternal(newValue.Value, oldValue.Value, index);
            }
            RaiseCollectionReplace(newValue, oldValue, index);
        }

        protected virtual void MoveItem(TKey key, int newIndex)
        {
            if (newIndex < 0 || newIndex >= this.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            var oldIndex = KeySet[key];
            if (oldIndex == newIndex)
                return;
            var value = Items[oldIndex];
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, value);
            var start = Math.Min(oldIndex, newIndex);
            var end = Math.Max(oldIndex, newIndex);
            updateIndex(start, end - start + 1);
            if (this.keys != null)
            {
                this.keys.RaiseCollectionMoveInternal(value.Key, newIndex, oldIndex);
            }
            if (this.values != null)
            {
                this.values.RaiseCollectionMoveInternal(value.Value, newIndex, oldIndex);
            }
            RaiseCollectionMove(value, newIndex, oldIndex);
        }

        protected virtual void ClearItems()
        {
            Items.Clear();
            KeySet.Clear();
            if (this.keys != null)
            {
                this.keys.RaiseCountChangedInternal();
                this.keys.RaiseCollectionResetInternal();
            }
            if (this.values != null)
            {
                this.values.RaiseCountChangedInternal();
                this.values.RaiseCollectionResetInternal();
            }
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionReset();
        }

        private void insert(TKey key, TValue value)
        {
            if (KeySet.ContainsKey(key))
                SetItem(key, value);
            else
                InsertItem(key, value, Items.Count);
        }

        public void Move(TKey key, int newIndex)
        {
            MoveItem(key, newIndex);
        }

        public TValue this[TKey key]
        {
            get => Items[KeySet[key]].Value;
            set => insert(key, value);
        }
        object IDictionary.this[object key]
        {
            get => Items[KeySet[Helpers.CastKey<TKey>(key)]].Value;
            set => insert(Helpers.CastKey<TKey>(key), Helpers.CastValue<TValue>(value));
        }

        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get => Items[index];
            set
            {
                var oldValue = Items[index];
                RemoveItem(oldValue.Key);
                InsertItem(value.Key, value.Value, index);
            }
        }
        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
        object IList.this[int index]
        {
            get => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
            set => ((IList<KeyValuePair<TKey, TValue>>)this)[index] = Helpers.CastKVP<TKey, TValue>(value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ObservableKeyCollection keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ObservableValueCollection values;

        public ObservableKeyCollection Keys
            => LazyInitializer.EnsureInitialized(ref this.keys, () => new ObservableKeyCollection(this));
        public ObservableValueCollection Values
            => LazyInitializer.EnsureInitialized(ref this.values, () => new ObservableValueCollection(this));

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Keys => Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Values => Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public int Count => Items.Count;

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

        public void Add(TKey key, TValue value) => InsertItem(key, value, Items.Count);

        public bool ContainsKey(TKey key) => KeySet.ContainsKey(key);

        public bool ContainsValue(TValue value)
        {
            var c = EqualityComparer<TValue>.Default;
            foreach (var item in Items)
            {
                if (c.Equals(item.Value, value))
                    return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (!KeySet.ContainsKey(key))
                return false;
            RemoveItem(key);
            return true;
        }

        private ObservableDictionaryView<TKey, TValue> readOnlyView;

        public ObservableDictionaryView<TKey, TValue> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, () 
                => new ObservableDictionaryView<TKey, TValue>(this));

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!KeySet.TryGetValue(key, out var index))
            {
                value = default(TValue);
                return false;
            }
            value = Items[index].Value;
            return true;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => insert(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Items).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Items).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!KeySet.TryGetValue(item.Key, out var index))
                return false;
            var oldItem = Items[index];
            if (!EqualityComparer<TValue>.Default.Equals(oldItem.Value, item.Value))
                return false;
            return Remove(item.Key);
        }

        public List<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator() => Items.GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IDictionary.Add(object key, object value) => Add(Helpers.CastKey<TKey>(key), Helpers.CastValue<TValue>(value));

        bool IDictionary.Contains(object key) => ContainsKey(Helpers.CastKey<TKey>(key));

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly ObservableDictionary<TKey, TValue> parents;

            private readonly List<KeyValuePair<TKey, TValue>>.Enumerator enumlator;

            internal DictionaryEnumerator(ObservableDictionary<TKey, TValue> parents)
            {
                this.parents = parents;
                this.enumlator = parents.Items.GetEnumerator();
            }

            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            public object Key => this.enumlator.Current.Key;

            public object Value => this.enumlator.Current.Value;

            public object Current => this.enumlator.Current;

            public bool MoveNext() => this.enumlator.MoveNext();

            public void Reset() => ((IEnumerator)this.enumlator).Reset();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator(this);

        void IDictionary.Remove(object key) => Remove(Helpers.CastKey<TKey>(key));

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            if (!KeySet.TryGetValue(item.Key, out var index))
                return -1;
            var kv = Items[index];
            if (EqualityComparer<TValue>.Default.Equals(kv.Value, item.Value))
                return index;
            return -1;
        }

        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            if (!KeySet.TryGetValue(item.Key, out var currentIndex))
            {
                InsertItem(item.Key, item.Value, index);
                return;
            }
            SetItem(item.Key, item.Value);
            MoveItem(item.Key, index);
        }

        void IList<KeyValuePair<TKey, TValue>>.RemoveAt(int index)
        {
            var kv = Items[index];
            RemoveItem(kv.Key);
        }

        int IList.Add(object value)
        {
            ((IList<KeyValuePair<TKey, TValue>>)this).Add(Helpers.CastKVP<TKey, TValue>(value));
            return this.Count - 1;
        }

        bool IList.Contains(object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Contains(Helpers.CastKVP<TKey, TValue>(value));

        int IList.IndexOf(object value) => ((IList<KeyValuePair<TKey, TValue>>)this).IndexOf(Helpers.CastKVP<TKey, TValue>(value));

        void IList.Insert(int index, object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Insert(index, Helpers.CastKVP<TKey, TValue>(value));

        void IList.Remove(object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Remove(Helpers.CastKVP<TKey, TValue>(value));

        void IList.RemoveAt(int index) => ((IList<KeyValuePair<TKey, TValue>>)this).RemoveAt(index);

        public void Clear() => ClearItems();
    }
}
