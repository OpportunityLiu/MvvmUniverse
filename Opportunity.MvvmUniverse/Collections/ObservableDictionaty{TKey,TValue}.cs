using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    public class ObservableDictionaty<TKey, TValue>
        : ObservableObject
        , IObservableMap<TKey, TValue>
        , IDictionary<TKey, TValue>, IDictionary
        , ICollection<KeyValuePair<TKey, TValue>>
    {
        protected Dictionary<TKey, TValue> Items { get; }

        public ObservableDictionaty() : this(EqualityComparer<TKey>.Default) { }

        public ObservableDictionaty(IEqualityComparer<TKey> comparer)
        {
            this.Items = new Dictionary<TKey, TValue>(comparer);
        }

        public event MapChangedEventHandler<TKey, TValue> MapChanged;

        protected sealed class MapChangedEventArgs : IMapChangedEventArgs<TKey>
        {
            public MapChangedEventArgs(CollectionChange collectionChange, TKey key)
            {
                this.CollectionChange = collectionChange;
                this.Key = key;
            }

            public CollectionChange CollectionChange { get; private set; }

            public TKey Key { get; private set; }
        }

        protected void RaiseMapChanged(MapChangedEventArgs args)
        {
            var temp = MapChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, args);
            });
        }

        public void Add(TKey key, TValue value)
        {
            Items.Add(key, value);
            RaiseMapChanged(new MapChangedEventArgs(CollectionChange.ItemInserted, key));
            RaisePropertyChanged(nameof(Count));
        }

        public bool ContainsKey(TKey key)
            => Items.ContainsKey(key);

        public bool Remove(TKey key)
        {
            var r = Items.Remove(key);
            if (r == false)
                return false;
            RaiseMapChanged(new MapChangedEventArgs(CollectionChange.ItemRemoved, key));
            RaisePropertyChanged(nameof(Count));
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
            => Items.TryGetValue(key, out value);

        public TValue this[TKey key]
        {
            get => Items[key];
            set
            {
                if (Items.ContainsKey(key))
                {
                    Items[key] = value;
                    RaiseMapChanged(new MapChangedEventArgs(CollectionChange.ItemChanged, key));
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys => Items.Keys;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Items.Keys;

        public Dictionary<TKey, TValue>.ValueCollection Values => Items.Values;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Items.Values;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Items.Clear();
            RaiseMapChanged(new MapChangedEventArgs(CollectionChange.Reset, default(TKey)));
            RaisePropertyChanged(nameof(Count));
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Items).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Items).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Items.TryGetValue(item.Key, out var value))
            {
                if (EqualityComparer<TValue>.Default.Equals(value, item.Value))
                {
                    return Remove(item.Key);
                }
            }
            return false;
        }

        public int Count => Items.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        ICollection IDictionary.Keys => Items.Keys;

        ICollection IDictionary.Values => Items.Values;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

        object IDictionary.this[object key]
        {
            get => ((IDictionary)Items)[key];
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                if (!(key is TKey k))
                    throw new ArgumentException("Wrong type of key.", nameof(key));
                if (!(value is TValue v))
                    throw new ArgumentException("Wrong type of value.", nameof(value));
                this[k] = v;
            }
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
            => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Items.GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator()
            => ((IDictionary)Items).GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => Items.GetEnumerator();

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (!(key is TKey k))
                throw new ArgumentException("Wrong type of key.", nameof(key));
            if (!(value is TValue v))
                throw new ArgumentException("Wrong type of value.", nameof(value));
            Add(k, v);
        }

        bool IDictionary.Contains(object key)
            => ((IDictionary)Items).Contains(key);

        void IDictionary.Remove(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (!(key is TKey k))
                throw new ArgumentException("Wrong type of key.", nameof(key));
            Remove(k);
        }

        void ICollection.CopyTo(Array array, int index)
            => ((ICollection)Items).CopyTo(array, index);
    }
}
