﻿using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
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
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableDictionaryView<TKey, TValue> : ObservableCollectionBase<KeyValuePair<TKey, TValue>>
        , IReadOnlyDictionary<TKey, TValue>, IOrderedDictionary
        , IReadOnlyList<KeyValuePair<TKey, TValue>>, IList, ICollection<KeyValuePair<TKey, TValue>>
    {
        protected ObservableDictionary<TKey, TValue> Dictionary { get; }

        public ObservableDictionaryView(ObservableDictionary<TKey, TValue> dictionary)
        {
            this.Dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            dictionary.CollectionChanged += this.Dictionary_CollectionChanged;
        }

        private void Dictionary_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionChanged(e);
        }

        public TValue this[TKey key] => Dictionary[key];

        public KeyValuePair<TKey, TValue> ItemAt(int index) => Dictionary.ItemAt(index);

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index]
            => ((IList<KeyValuePair<TKey, TValue>>)Dictionary)[index];

        object IDictionary.this[object key]
        {
            get => ((IDictionary)Dictionary)[key];
            set => ThrowForReadOnlyCollection(Dictionary.ToString());
        }
        object IOrderedDictionary.this[int index]
        {
            get => ((IOrderedDictionary)Dictionary)[index];
            set => ThrowForReadOnlyCollection(Dictionary.ToString());
        }
        object IList.this[int index]
        {
            get => ((IList)Dictionary)[index];
            set => ThrowForReadOnlyCollection(Dictionary.ToString());
        }

        public ObservableDictionary<TKey, TValue>.ObservableKeyValueCollection<TKey> Keys => Dictionary.Keys;

        public ObservableDictionary<TKey, TValue>.ObservableKeyValueCollection<TValue> Values => Dictionary.Values;

        public int Count => Dictionary.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsFixedSize => ((IDictionary)Dictionary).IsFixedSize;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => ((IList)Dictionary).IsFixedSize;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IDictionary.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Keys => Dictionary.Keys;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
            => ((IReadOnlyDictionary<TKey, TValue>)Dictionary).Keys;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Values => Dictionary.Values;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
            => ((IReadOnlyDictionary<TKey, TValue>)Dictionary).Values;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => ((ICollection)Dictionary).IsSynchronized;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Dictionary).SyncRoot;

        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

        public bool ConatinsValue(TValue value) => Dictionary.ContainsValue(value);

        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

        void IDictionary.Add(object key, object value) => ThrowForReadOnlyCollection(Dictionary.ToString());

        int IList.Add(object value) => ThrowForReadOnlyCollection<int>(Dictionary.ToString());

        void IDictionary.Clear() => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IList.Clear() => ThrowForReadOnlyCollection(Dictionary.ToString());

        bool IDictionary.Contains(object key) => ((IDictionary)Dictionary).Contains(key);

        bool IList.Contains(object value) => ((IList)Dictionary).Contains(value);

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Dictionary).CopyTo(array, index);

        public int IndexOfKey(TKey key) => Dictionary.IndexOfKey(key);

        public int IndexOfValue(TValue value) => Dictionary.IndexOfValue(value);

        int IList.IndexOf(object value) => ((IList)Dictionary).IndexOf(value);

        void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IDictionary.Remove(object key) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IList.Remove(object value) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => ThrowForReadOnlyCollection(Dictionary.ToString());

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            => ThrowForReadOnlyCollection<bool>(Dictionary.ToString());

        public ObservableDictionary<TKey, TValue>.DictionaryEnumerator GetEnumerator()
            => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => ((IEnumerable<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)Dictionary).GetEnumerator();
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => ((IDictionary)Dictionary).GetEnumerator();

        void IOrderedDictionary.Insert(int index, object key, object value) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IOrderedDictionary.RemoveAt(int index) => ThrowForReadOnlyCollection(Dictionary.ToString());
    }
}
