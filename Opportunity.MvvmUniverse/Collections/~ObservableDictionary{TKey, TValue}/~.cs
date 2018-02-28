﻿using Opportunity.MvvmUniverse.Collections.Internal;
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
    /// <summary>
    /// Ordered generic dictionary can notify observers when changes happens.
    /// </summary>
    /// <typeparam name="TKey">type of key</typeparam>
    /// <typeparam name="TValue">type of value</typeparam>
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class ObservableDictionary<TKey, TValue>
        : ObservableCollectionBase<KeyValuePair<TKey, TValue>>
        , IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IOrderedDictionary
        , IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
    {
        private List<TKey> keyItems;
        private List<TValue> valueItems;
        /// <summary>
        /// Ordered keys.
        /// </summary>
        protected List<TKey> KeyItems => LazyInitializer.EnsureInitialized(ref this.keyItems);
        /// <summary>
        /// Ordered values.
        /// </summary>
        protected List<TValue> ValueItems => LazyInitializer.EnsureInitialized(ref this.valueItems);
        /// <summary>
        /// Mapping from key to index of <see cref="KeyItems"/> and <see cref="ValueItems"/>.
        /// </summary>
        protected Dictionary<TKey, int> KeySet { get; }

        /// <summary>
        /// Comparer used to compare keys.
        /// </summary>
        public IEqualityComparer<TKey> Comparer => KeySet.Comparer;

        /// <summary>
        /// Create new instance of <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public ObservableDictionary() : this(null, null) { }
        /// <summary>
        /// Create new instance of <see cref="ObservableDictionary{TKey, TValue}"/>
        /// with specific <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> to compare keys.</param>
        public ObservableDictionary(IEqualityComparer<TKey> comparer) : this(null, comparer) { }
        /// <summary>
        /// Create new instance of <see cref="ObservableDictionary{TKey, TValue}"/>
        /// by copying data from <paramref name="dictionary"/>.
        /// </summary>
        /// <param name="dictionary">An <see cref="IDictionary{TKey, TValue}"/> to copy data from.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }
        /// <summary>
        /// Create new instance of <see cref="ObservableDictionary{TKey, TValue}"/>
        /// by copying data from <paramref name="dictionary"/>
        /// with specific <paramref name="comparer"/>.
        /// </summary>
        /// <param name="dictionary">An <see cref="IDictionary{TKey, TValue}"/> to copy data from.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> to compare keys.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
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

        /// <summary>
        /// Insert new key-value pair to given <paramref name="index"/> of the dictionary.
        /// </summary>
        /// <param name="key">Key to insert.</param>
        /// <param name="value">Value to insert.</param>
        /// <param name="index">Position of insertion.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> out of range of the dictionary.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> found in the dictionay.</exception>
        protected virtual void InsertItem(int index, TKey key, TValue value)
        {
            check();
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

        /// <summary>
        /// Remove key-value pair of given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> found and removed, otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        protected virtual bool RemoveItem(TKey key)
        {
            check();
            if (!KeySet.TryGetValue(key, out var removedIndex))
                return false;
            var removedValue = ValueItems[removedIndex];
            KeySet.Remove(key);
            KeyItems.RemoveAt(removedIndex);
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
            return true;
        }

        /// <summary>
        /// Set <paramref name="value"/> of given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to set</param>
        /// <param name="value">Value to set</param>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> not found in the dictionay.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        protected virtual void SetItem(TKey key, TValue value)
        {
            check();
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

        /// <summary>
        /// Move key-value pair with the given <paramref name="key"/> to <paramref name="newIndex"/>.
        /// </summary>
        /// <param name="key">Key of key-value pair to move.</param>
        /// <param name="newIndex">New position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> not found in the dictionay.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> out of range of the dictionary.</exception>
        protected virtual void MoveItem(TKey key, int newIndex)
        {
            check();
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

        /// <summary>
        /// Clear all key-value pairs in the dictionary.
        /// </summary>
        protected virtual void ClearItems()
        {
            check();
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

        /// <summary>
        /// Move key-value pair with the given <paramref name="key"/> to <paramref name="newIndex"/>.
        /// </summary>
        /// <param name="key">Key of key-value pair to move.</param>
        /// <param name="newIndex">New position.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> not found in the dictionay.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> out of range of the dictionary.</exception>
        public void Move(TKey key, int newIndex) => MoveItem(key, newIndex);

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
            get => ValueItems[KeySet[key]];
            set => setOrAdd(key, value);
        }
        /// <summary>
        /// Get key-value pair at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of key-value pair.</param>
        /// <returns>Key-value pair at <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> out of range of the dictionary.</exception>
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

        /// <inheritdoc/>
        public ObservableKeyCollection Keys
            => LazyInitializer.EnsureInitialized(ref this.keys, () => new ObservableKeyCollection(this));
        /// <inheritdoc/>
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
        /// <summary>
        /// Get a read-only view of current instance.
        /// </summary>
        /// <returns>A read-only view of current instance.</returns>
        public ObservableDictionaryView<TKey, TValue> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, ReadOnlyViewFactory);

        /// <summary>
        /// This method will be called when <see cref="AsReadOnly()"/> first called on this instance.
        /// </summary>
        protected virtual ObservableDictionaryView<TKey, TValue> ReadOnlyViewFactory()
            => new ObservableDictionaryView<TKey, TValue>(this);

        /// <inheritdoc/>
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

        /// <summary>
        /// Add new key-value pair at the end of the dictionary.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> found in the dictionay.</exception>
        public void Add(TKey key, TValue value) => InsertItem(Count, key, value);
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        void IDictionary.Add(object key, object value) => Add(CastKey<TKey>(key), CastValue<TValue>(value));
        int IList.Add(object value)
        {
            ((IList<KeyValuePair<TKey, TValue>>)this).Add(CastKVP<TKey, TValue>(value));
            return this.Count - 1;
        }

        /// <summary>
        /// Insert new key-value pair to given <paramref name="index"/> of the dictionary.
        /// </summary>
        /// <param name="key">Key to insert.</param>
        /// <param name="value">Value to insert.</param>
        /// <param name="index">Position of insertion.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> out of range of the dictionary.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> found in the dictionay.</exception>
        public void Insert(int index, TKey key, TValue value) => InsertItem(index, key, value);
        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
            => Insert(index, item.Key, item.Value);
        void IList.Insert(int index, object value) => ((IList<KeyValuePair<TKey, TValue>>)this).Insert(index, CastKVP<TKey, TValue>(value));
        void IOrderedDictionary.Insert(int index, object key, object value)
            => Insert(index, CastKey<TKey>(key), CastValue<TValue>(value));

        /// <inheritdoc/>
        public bool ContainsKey(TKey key) => KeySet.ContainsKey(key);
        /// <inheritdoc/>
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

        /// <summary>
        /// Remove key-value pair of given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> found and removed, otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public bool Remove(TKey key) => RemoveItem(key);
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

        /// <summary>
        /// Remove key-value pair at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index of key-value pair to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> out of range of the dictionary.</exception>
        public void RemoveAt(int index)
        {
            var key = KeyItems[index];
            RemoveItem(key);
        }

        /// <summary>
        /// Try get key-value pair in the dictionary.
        /// </summary>
        /// <param name="key">Key to find in the dictionary.</param>
        /// <param name="value">Value of the <paramref name="key"/>, or default value, if key not found.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> found in the dictionary, otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value, out _);
        /// <summary>
        /// Try get key-value pair and its index in the dictionary.
        /// </summary>
        /// <param name="key">Key to find in the dictionary.</param>
        /// <param name="value">Value of the <paramref name="key"/>, or default value, if key not found.</param>
        /// <param name="index">Index of the key-value pair, or default value, if key not found.</param>
        /// <returns><see langword="true"/> if <paramref name="key"/> found in the dictionary, otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value, out int index)
        {
            if (!KeySet.TryGetValue(key, out index))
            {
                value = default;
                return false;
            }
            value = ValueItems[index];
            return true;
        }

        /// <inheritdoc/>
        public DictionaryEnumerator GetEnumerator()
            => new DictionaryEnumerator(this, DictionaryEnumerator.Type.IEnumeratorKVP);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => new DictionaryEnumerator(this, DictionaryEnumerator.Type.IEnumeratorKVP);
        IEnumerator IEnumerable.GetEnumerator()
            => new DictionaryEnumerator(this, DictionaryEnumerator.Type.IEnumeratorKVP);
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
            => new DictionaryEnumerator(this, DictionaryEnumerator.Type.IDictionaryEnumerator);
        IDictionaryEnumerator IDictionary.GetEnumerator()
            => new DictionaryEnumerator(this, DictionaryEnumerator.Type.IDictionaryEnumerator);

        /// <summary>
        /// Iterate all key-value pairs in the dictionary.
        /// </summary>
        /// <param name="action">Action for each key-value pair.</param>
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
        /// <summary>
        /// Iterate all key-value pairs and their index in the dictionary.
        /// </summary>
        /// <param name="action">Action for each key-value pair and its index.</param>
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

        /// <summary>
        /// Enumerator of <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public struct DictionaryEnumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>
        {
            internal enum Type { Unknown = 0, IDictionaryEnumerator, IEnumeratorKVP }

            private List<TKey>.Enumerator keyEnumerator;
            private List<TValue>.Enumerator valueEnumerator;
            private Type type;

            internal DictionaryEnumerator(ObservableDictionary<TKey, TValue> parent, Type type)
            {
                this.keyEnumerator = parent.KeyItems.GetEnumerator();
                this.valueEnumerator = parent.ValueItems.GetEnumerator();
                this.type = type;
            }

            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Key, Value);

            /// <inheritdoc/>
            public TKey Key => this.keyEnumerator.Current;
            /// <inheritdoc/>
            public TValue Value => this.valueEnumerator.Current;

            object IDictionaryEnumerator.Key => Key;
            object IDictionaryEnumerator.Value => Value;

            /// <inheritdoc/>
            public KeyValuePair<TKey, TValue> Current => CreateKVP(Key, Value);
            object IEnumerator.Current
            {
                get
                {
                    switch (this.type)
                    {
                    case Type.IDictionaryEnumerator: return new DictionaryEntry(Key, Value);
                    case Type.IEnumeratorKVP: return Current;
                    default: throw new InvalidOperationException();
                    }
                }
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                this.keyEnumerator.Dispose();
                this.valueEnumerator.Dispose();
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                var kr = this.keyEnumerator.MoveNext();
                var vr = this.valueEnumerator.MoveNext();
                if (kr == vr)
                    return kr;
                this.Dispose();
                throw new InvalidOperationException("Dictionary has been changed.");
            }

            private static void reset<T>(ref T enumerator)
                where T : IEnumerator
            {
                enumerator.Reset();
            }

            /// <inheritdoc/>
            public void Reset()
            {
                reset(ref this.keyEnumerator);
                reset(ref this.valueEnumerator);
            }
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Clear all key-value pairs in the dictionary.
        /// </summary>
        public void Clear() => ClearItems();
    }
}
