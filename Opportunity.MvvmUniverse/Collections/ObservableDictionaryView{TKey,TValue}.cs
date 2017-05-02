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
    public class ObservableDictionaryView<TKey, TValue> : ObservableCollectionBase
        , IReadOnlyDictionary<TKey, TValue>, IDictionary
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

        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index]
            => ((IList<KeyValuePair<TKey, TValue>>)Dictionary)[index];

        object IDictionary.this[object key]
        {
            get => ((IDictionary)Dictionary)[key];
            set => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());
        }
        object IList.this[int index]
        {
            get => ((IList)Dictionary)[index];
            set => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());
        }

        public ObservableDictionary<TKey, TValue>.ObservableKeyCollection Keys => Dictionary.Keys;

        public ObservableDictionary<TKey, TValue>.ObservableValueCollection Values => Dictionary.Values;

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

        void IDictionary.Add(object key, object value) => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        int IList.Add(object value) => Helpers.ThrowForReadOnlyCollection<int>(Dictionary.ToString());

        void IDictionary.Clear() => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        void IList.Clear() => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        bool IDictionary.Contains(object key) => ((IDictionary)Dictionary).Contains(key);

        bool IList.Contains(object value) => ((IList)Dictionary).Contains(value);

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Dictionary).CopyTo(array, index);

        int IList.IndexOf(object value) => ((IList)Dictionary).IndexOf(value);

        void IList.Insert(int index, object value) => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        void IDictionary.Remove(object key) => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        void IList.Remove(object value) => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        void IList.RemoveAt(int index) => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => Helpers.ThrowForReadOnlyCollection(Dictionary.ToString());

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            => Helpers.ThrowForReadOnlyCollection<bool>(Dictionary.ToString());

        public List<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator() => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => ((IEnumerable<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)Dictionary).GetEnumerator();
    }
}
