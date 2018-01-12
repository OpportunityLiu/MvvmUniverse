using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
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
    public partial class ObservableDictionary<TKey, TValue>
        : ObservableCollectionBase<KeyValuePair<TKey, TValue>>
        , IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IOrderedDictionary
        , IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
    {
        private List<TKey> keyItems;
        private List<TValue> valueItems;
        protected List<TKey> KeyItems => LazyInitializer.EnsureInitialized(ref this.keyItems);
        protected List<TValue> ValueItems => LazyInitializer.EnsureInitialized(ref this.valueItems);
        protected Dictionary<TKey, int> KeySet { get; }

        public IEqualityComparer<TKey> Comparer => KeySet.Comparer;

        public ObservableDictionary() : this(null, EqualityComparer<TKey>.Default) { }
        public ObservableDictionary(IEqualityComparer<TKey> comparer) : this(null, comparer) { }
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, EqualityComparer<TKey>.Default) { }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            this.KeySet = new Dictionary<TKey, int>(comparer);
            if (dictionary != null)
            {
                foreach (var item in dictionary)
                {
                    this.Add(item.Key, item.Value);
                }
            }
        }

        private void updateIndex(int startIndex, int length)
        {
            for (var i = 0; i < length; i++)
            {
                var keyitem = KeyItems[startIndex];
                KeySet[keyitem] = startIndex;
                startIndex++;
            }
        }

        [Conditional("DEBUG")]
        private void check()
        {
            if (KeySet.Count == 0)
            {
                Debug.Assert(this.keyItems == null || this.keyItems.Count == 0, "KeyItems is not null or empty.");
                Debug.Assert(this.valueItems == null || this.valueItems.Count == 0, "ValueItems is not null or empty.");
            }
            else
            {
                var c = KeySet.Count;
                Debug.Assert(KeyItems.Count == c, "KeyItems.Count != KeySet.Count");
                Debug.Assert(ValueItems.Count == c, "ValueItems.Count != KeySet.Count");
                for (var i = 0; i < KeyItems.Count; i++)
                {
                    Debug.Assert(KeySet[KeyItems[i]] == i, $"Key in KeySet and KeyItems not match at {i}");
                }
            }
        }

        protected virtual void InsertItem(int index, TKey key, TValue value)
        {
            if (index < 0 || index > KeySet.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            KeySet.Add(key, index);
            KeyItems.Insert(index, key);
            ValueItems.Insert(index, value);
            updateIndex(index + 1, KeyItems.Count - index - 1);
            if (this.keys != null)
            {
                this.keys.RaiseCountChangedInternal();
                this.keys.RaiseCollectionAddInternal(key, KeyItems.Count - 1);
            }
            if (this.values != null)
            {
                this.values.RaiseCountChangedInternal();
                this.values.RaiseCollectionAddInternal(value, ValueItems.Count - 1);
            }
            OnPropertyChanged(nameof(Count));
            OnCollectionAdd(CreateKVP(key, value), KeySet.Count - 1);
            check();
        }

        protected virtual void RemoveItem(TKey key)
        {
            var removedIndex = KeySet[key];
            KeySet.Remove(key);
            KeyItems.RemoveAt(removedIndex);
            var removedValue = ValueItems[removedIndex];
            ValueItems.RemoveAt(removedIndex);
            updateIndex(removedIndex, KeyItems.Count - removedIndex);
            if (this.keys != null)
            {
                this.keys.RaiseCountChangedInternal();
                this.keys.RaiseCollectionRemoveInternal(key, removedIndex);
            }
            if (this.values != null)
            {
                this.values.RaiseCountChangedInternal();
                this.values.RaiseCollectionRemoveInternal(removedValue, removedIndex);
            }
            OnPropertyChanged(nameof(Count));
            OnCollectionRemove(CreateKVP(key, removedValue), removedIndex);
            check();
        }

        protected virtual void SetItem(TKey key, TValue value)
        {
            var index = KeySet[key];
            var oldValue = ValueItems[index];
            ValueItems[index] = value;
            // Key collection will not change.
            if (this.values != null)
            {
                this.values.RaiseCollectionReplaceInternal(value, oldValue, index);
            }
            OnCollectionReplace(CreateKVP(key, value), CreateKVP(key, oldValue), index);
            check();
        }

        protected virtual void MoveItem(TKey key, int newIndex)
        {
            if (newIndex < 0 || newIndex >= this.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            var oldIndex = KeySet[key];
            if (oldIndex == newIndex)
                return;
            var value = ValueItems[oldIndex];
            ValueItems.RemoveAt(oldIndex);
            ValueItems.Insert(newIndex, value);
            KeyItems.RemoveAt(oldIndex);
            KeyItems.Insert(newIndex, key);
            var start = Math.Min(oldIndex, newIndex);
            var end = Math.Max(oldIndex, newIndex);
            updateIndex(start, end - start + 1);
            if (this.keys != null)
            {
                this.keys.RaiseCollectionMoveInternal(key, newIndex, oldIndex);
            }
            if (this.values != null)
            {
                this.values.RaiseCollectionMoveInternal(value, newIndex, oldIndex);
            }
            OnCollectionMove(CreateKVP(key, value), newIndex, oldIndex);
            check();
        }

        protected virtual void ClearItems()
        {
            this.keyItems?.Clear();
            this.valueItems?.Clear();
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
            OnPropertyChanged(nameof(Count));
            OnCollectionReset();
            check();
        }

        private void setOrAdd(TKey key, TValue value)
        {
            if (KeySet.ContainsKey(key))
                SetItem(key, value);
            else
                InsertItem(Count, key, value);
        }

        public void Move(TKey key, int newIndex) => MoveItem(key, newIndex);

        public TValue this[TKey key]
        {
            get => ValueItems[KeySet[key]];
            set => setOrAdd(key, value);
        }
        public KeyValuePair<TKey, TValue> ItemAt(int index) => CreateKVP(KeyItems[index], ValueItems[index]);
        object IDictionary.this[object key]
        {
            get => this[CastKey<TKey>(key)];
            set => setOrAdd(CastKey<TKey>(key), CastValue<TValue>(value));
        }
        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get => ItemAt(index);
            set
            {
                var oldKey = KeyItems[index];
                if (Comparer.Equals(oldKey, value.Key))
                    SetItem(value.Key, value.Value);
                else if (ContainsKey(value.Key))
                    throw new InvalidOperationException($"Item of same key is at a position other than index({index}) of the ObservableDictionary");
                else
                {
                    RemoveItem(oldKey);
                    InsertItem(index, value.Key, value.Value);
                }
            }
        }
        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index]
            => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
        object IList.this[int index]
        {
            get => ((IList<KeyValuePair<TKey, TValue>>)this)[index];
            set => ((IList<KeyValuePair<TKey, TValue>>)this)[index] = CastKVP<TKey, TValue>(value);
        }
        object IOrderedDictionary.this[int index]
        {
            get => ValueItems[index];
            set => SetItem(KeyItems[index], CastValue<TValue>(value));
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

        private ObservableDictionaryView<TKey, TValue> readOnlyView;
        public ObservableDictionaryView<TKey, TValue> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, ()
                => new ObservableDictionaryView<TKey, TValue>(this));

        public int Count => KeySet.Count;

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
        object ICollection.SyncRoot => ((ICollection)this.KeySet).SyncRoot;

        public void Add(TKey key, TValue value) => InsertItem(Count, key, value);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        void IDictionary.Add(object key, object value) => Add(CastKey<TKey>(key), CastValue<TValue>(value));
        int IList.Add(object value)
        {
            ((IList<KeyValuePair<TKey, TValue>>)this).Add(CastKVP<TKey, TValue>(value));
            return this.Count - 1;
        }

        public void Insert(int index, TKey key, TValue value) => InsertItem(index, key, value);
        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
            => Insert(index, item.Key, item.Value);
        void IList.Insert(int index, object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Insert(index, CastKVP<TKey, TValue>(value));
        void IOrderedDictionary.Insert(int index, object key, object value)
            => Insert(index, CastKey<TKey>(key), CastValue<TValue>(value));

        public bool ContainsKey(TKey key) => KeySet.ContainsKey(key);
        public bool ContainsValue(TValue value) => ValueItems.Contains(value);
        bool IDictionary.Contains(object key) => ((IDictionary)KeySet).Contains(key);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (TryGetValue(item.Key, out var value))
            {
                return EqualityComparer<TValue>.Default.Equals(item.Value, value);
            }
            return false;
        }
        bool IList.Contains(object value)
        {
            try
            {
                return ((IList<KeyValuePair<TKey, TValue>>)this).Contains(CastKVP<TKey, TValue>(value));
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public bool Remove(TKey key)
        {
            if (!KeySet.ContainsKey(key))
                return false;
            RemoveItem(key);
            return true;
        }
        void IDictionary.Remove(object key) => Remove(CastKey<TKey>(key));
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!KeySet.TryGetValue(item.Key, out var index))
                return false;
            var oldValue = ValueItems[index];
            if (!EqualityComparer<TValue>.Default.Equals(oldValue, item.Value))
                return false;
            RemoveItem(item.Key);
            return true;
        }
        void IList.Remove(object value)
            => ((IList<KeyValuePair<TKey, TValue>>)this).Remove(CastKVP<TKey, TValue>(value));

        public void RemoveAt(int index)
        {
            var key = KeyItems[index];
            RemoveItem(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!KeySet.TryGetValue(key, out var index))
            {
                value = default(TValue);
                return false;
            }
            value = ValueItems[index];
            return true;
        }

        public DictionaryEnumerator GetEnumerator() => new DictionaryEnumerator(this, 0);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => new DictionaryEnumerator(this, 0);
        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IOrderedDictionary)this).GetEnumerator();

        public void ForEach(Action<TKey, TValue> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            using (var e = this.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    action(e.Key, e.Value);
                }
            }
        }
        public void ForEach(Action<int, TKey, TValue> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            var i = 0;
            using (var e = this.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    action(i, e.Key, e.Value);
                    i++;
                }
            }
        }

        public struct DictionaryEnumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private List<TKey>.Enumerator keyEnumerator;
            private List<TValue>.Enumerator valueEnumerator;
            private int type;

            internal DictionaryEnumerator(ObservableDictionary<TKey, TValue> parent, int type)
            {
                this.keyEnumerator = parent.KeyItems.GetEnumerator();
                this.valueEnumerator = parent.ValueItems.GetEnumerator();
                this.type = type;
            }

            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Key, Value);

            public TKey Key => this.keyEnumerator.Current;
            public TValue Value => this.valueEnumerator.Current;

            object IDictionaryEnumerator.Key => Key;
            object IDictionaryEnumerator.Value => Value;

            public KeyValuePair<TKey, TValue> Current => CreateKVP(Key, Value);
            object IEnumerator.Current => this.type == 0 ? (object)Current : new DictionaryEntry(Key, Value);

            public void Dispose()
            {
                this.keyEnumerator.Dispose();
                this.valueEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                var kr = this.keyEnumerator.MoveNext();
                var vr = this.valueEnumerator.MoveNext();
                if (kr == vr)
                    return kr;
                throw new InvalidOperationException("Dictionary has been changed.");
            }

            private static void reset<T>(ref T enumerator)
                where T : IEnumerator
            {
                enumerator.Reset();
            }

            public void Reset()
            {
                reset(ref this.keyEnumerator);
                reset(ref this.valueEnumerator);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (this.Count + arrayIndex > array.Length)
                throw new ArgumentException("Not enough space in array");
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }
        void ICollection.CopyTo(Array array, int index)
            => CopyTo((KeyValuePair<TKey, TValue>[])array, index);

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            if (!KeySet.TryGetValue(item.Key, out var index))
                return -1;
            var v = ValueItems[index];
            if (EqualityComparer<TValue>.Default.Equals(v, item.Value))
                return index;
            return -1;
        }
        int IList.IndexOf(object value)
        {
            try
            {
                return ((IList<KeyValuePair<TKey, TValue>>)this).IndexOf(CastKVP<TKey, TValue>(value));
            }
            catch (ArgumentException)
            {
                return -1;
            }
        }

        public void Clear() => ClearItems();
    }
}
