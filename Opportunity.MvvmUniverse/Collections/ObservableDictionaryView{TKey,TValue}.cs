using Opportunity.MvvmUniverse.Collections.Internal;
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
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(e);
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

        public ObservableDictionary<TKey, TValue>.ObservableKeyCollection Keys => Dictionary.Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Keys => Dictionary.Keys;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Dictionary.Keys;

        public ObservableDictionary<TKey, TValue>.ObservableValueCollection Values => Dictionary.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Values => Dictionary.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Dictionary.Values;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => ((ICollection)Dictionary).IsSynchronized;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Dictionary).SyncRoot;

        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
        public bool ConatinsValue(TValue value) => Dictionary.ContainsValue(value);
        bool IDictionary.Contains(object key) => ((IDictionary)Dictionary).Contains(key);
        bool IList.Contains(object value) => ((IList)Dictionary).Contains(value);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);

        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

        void IDictionary.Add(object key, object value) => ThrowForReadOnlyCollection(Dictionary.ToString());
        int IList.Add(object value) => ThrowForReadOnlyCollection<int>(Dictionary.ToString());
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IOrderedDictionary.Insert(int index, object key, object value) => ThrowForReadOnlyCollection(Dictionary.ToString());
        void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(Dictionary.ToString());

        void IDictionary.Clear() => ThrowForReadOnlyCollection(Dictionary.ToString());
        void IList.Clear() => ThrowForReadOnlyCollection(Dictionary.ToString());
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => ThrowForReadOnlyCollection(Dictionary.ToString());

        int IList.IndexOf(object value) => ((IList)Dictionary).IndexOf(value);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)Dictionary).CopyTo(array, index);

        void IDictionary.Remove(object key) => ThrowForReadOnlyCollection(Dictionary.ToString());
        void IList.Remove(object value) => ThrowForReadOnlyCollection(Dictionary.ToString());
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ThrowForReadOnlyCollection<bool>(Dictionary.ToString());
        void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(Dictionary.ToString());
        void IOrderedDictionary.RemoveAt(int index) => ThrowForReadOnlyCollection(Dictionary.ToString());

        public ObservableDictionary<TKey, TValue>.DictionaryEnumerator GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        IDictionaryEnumerator IDictionary.GetEnumerator() => GetEnumerator();
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() => GetEnumerator();

        public void ForEach(Action<TKey, TValue> action) => Dictionary.ForEach(action);
        public void ForEach(Action<int, TKey, TValue> action) => Dictionary.ForEach(action);
    }
}
