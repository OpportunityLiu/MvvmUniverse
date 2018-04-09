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
using Windows.Foundation.Collections;
using System.ComponentModel;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Ordered generic dictionary can notify observers when changes happens.
    /// </summary>
    /// <typeparam name="TKey">Type of keys.</typeparam>
    /// <typeparam name="TValue">Type of values.</typeparam>
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public partial class ObservableDictionary<TKey, TValue> : ObservableCollectionBase<KeyValuePair<TKey, TValue>>
        , IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IOrderedDictionary
        , IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>
        , ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
        , IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Ordered keys.
        /// </summary>
        protected List<TKey> KeyItems { get; } = new List<TKey>();
        /// <summary>
        /// Ordered values.
        /// </summary>
        protected List<TValue> ValueItems { get; } = new List<TValue>();
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
            KeySet = new Dictionary<TKey, int>(comparer);
            if (dictionary is null)
                return;
            foreach (var item in dictionary)
            {
                Add(item.Key, item.Value);
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
            var c = KeySet.Count;
            Debug.Assert(KeyItems.Count == c, "KeyItems.Count != KeySet.Count");
            Debug.Assert(ValueItems.Count == c, "ValueItems.Count != KeySet.Count");
            for (var i = 0; i < KeyItems.Count; i++)
            {
                Debug.Assert(KeySet[KeyItems[i]] == i, $"Key in KeySet and KeyItems not match at {i}");
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
            OnPropertyChanged(nameof(Count));
            OnItemInserted(KeySet.Count - 1);
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
            KeySet.Remove(key);
            KeyItems.RemoveAt(removedIndex);
            ValueItems.RemoveAt(removedIndex);
            updateIndex(removedIndex, KeyItems.Count - removedIndex);
            OnPropertyChanged(nameof(Count));
            OnItemRemoved(removedIndex);
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
            if (default(TValue) == null && ReferenceEquals(oldValue, value))
                return;
            KeyItems[index] = key;
            ValueItems[index] = value;
            OnItemChanged(index);
            check();
        }

        /// <summary>
        /// Clear all key-value pairs in the dictionary.
        /// </summary>
        protected virtual void ClearItems()
        {
            check();
            KeyItems.Clear();
            ValueItems.Clear();
            KeySet.Clear();
            OnPropertyChanged(nameof(Count));
            OnVectorReset();
            check();
        }

        /// <summary>
        /// Raise <see cref="ObservableObject.PropertyChanged"/> for this collection, <see cref="Keys"/> and <see cref="Values"/>.
        /// </summary>
        /// <param name="args">event args</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        /// <remarks>Will use <see cref="DispatcherHelper"/> to raise event on UI thread
        /// if <see cref="DispatcherHelper.UseForNotification"/> is <see langword="true"/>.</remarks>
        protected override void OnPropertyChanged(IEnumerable<PropertyChangedEventArgs> args)
        {
            base.OnPropertyChanged(args);
            this.keys?.RaisePropertyChangedInternal(args);
            this.values?.RaisePropertyChangedInternal(args);
        }

        /// <summary>
        /// Raise <see cref="ObservableCollectionBase{T}.VectorChanged"/> for this collection, <see cref="Keys"/> and <see cref="Values"/>.
        /// </summary>
        /// <param name="args">Event args.</param>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/></exception>
        /// <remarks>Will use <see cref="DispatcherHelper"/> to raise event on UI thread
        /// if <see cref="DispatcherHelper.UseForNotification"/> is <see langword="true"/>.</remarks>
        protected override void OnVectorChanged(IVectorChangedEventArgs args)
        {
            base.OnVectorChanged(args);
            this.keys?.RaiseVectorChangedInternal(args);
            this.values?.RaiseVectorChangedInternal(args);
        }

        private void setOrAdd(TKey key, TValue value)
        {
            if (KeySet.ContainsKey(key))
                SetItem(key, value);
            else
                InsertItem(Count, key, value);
        }

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
            get
            {
                if (TryGetValue(CastKey<TKey>(key), out var v))
                    return v;
                return default;
            }
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
            => new UndisposableObservableDictionaryView<TKey, TValue>(this);

        /// <inheritdoc/>
        public int Count => KeySet.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsFixedSize => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)KeySet).SyncRoot;

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
        void IOrderedDictionary.Insert(int index, object key, object value)
            => Insert(index, CastKey<TKey>(key), CastValue<TValue>(value));

        /// <inheritdoc/>
        public bool ContainsKey(TKey key) => KeySet.ContainsKey(key);
        bool IDictionary.Contains(object key) => ((IDictionary)KeySet).Contains(key);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (TryGetValue(item.Key, out var value))
            {
                return EqualityComparer<TValue>.Default.Equals(item.Value, value);
            }
            return false;
        }

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            if (!KeySet.TryGetValue(item.Key, out var index))
                return -1;
            var v = ValueItems[index];
            if (EqualityComparer<TValue>.Default.Equals(v, item.Value))
                return index;
            return -1;
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
            using (var e = GetEnumerator())
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
            using (var e = GetEnumerator())
            {
                while (e.MoveNext())
                {
                    action(i, e.Key, e.Value);
                    i++;
                }
            }
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (Count + arrayIndex > array.Length)
                throw new ArgumentException("Not enough space in array");
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        /// <summary>
        /// Clear all key-value pairs in the dictionary.
        /// </summary>
        public void Clear() => ClearItems();
    }
}
