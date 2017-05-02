using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class Mscorlib_DictionaryValueCollectionDebugView<TKey, TValue>
    {
        private ICollection<TValue> collection;

        public Mscorlib_DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                TValue[] items = new TValue[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    public partial class ObservableDictionary<TKey, TValue> : ObservableCollectionBase, IDictionary<TKey, TValue>, IDictionary, IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
    {
        [DebuggerTypeProxy(typeof(Mscorlib_DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableValueCollection : ObservableCollectionBase, IReadOnlyList<TValue>, ICollection<TValue>, IList
        {
            private readonly ObservableDictionary<TKey, TValue> parent;

            public ObservableValueCollection(ObservableDictionary<TKey, TValue> parent)
            {
                this.parent = parent;
                this.parent.CollectionChanged += this.Parent_CollectionChanged;
            }

            private void Parent_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                var newItem = e.NewItems == null ? default(KeyValuePair<TKey, TValue>) : e.NewItems.Cast<KeyValuePair<TKey, TValue>>().FirstOrDefault();
                var oldItem = e.OldItems == null ? default(KeyValuePair<TKey, TValue>) : e.OldItems.Cast<KeyValuePair<TKey, TValue>>().FirstOrDefault();
                switch (e.Action)
                {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    RaiseCollectionAdd(newItem.Value, e.NewStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    RaiseCollectionMove(newItem.Value, e.NewStartingIndex, e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RaiseCollectionRemove(oldItem.Value, e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    RaiseCollectionReplace(newItem.Value, oldItem.Value, e.NewStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RaiseCollectionReset();
                    break;
                }
            }

            public TValue this[int index] => this.parent[this.parent.SortedKeys[index]];

            object IList.this[int index]
            {
                get => this.parent[this.parent.SortedKeys[index]];
                set => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");
            }

            public int Count => this.parent.Count;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsFixedSize => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TValue>.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.parent.Items).SyncRoot;

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (var item in this.parent.SortedKeys)
                {
                    yield return this.parent[item];
                }
            }

            int IList.Add(object value) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void ICollection<TValue>.Add(TValue item) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void IList.Clear() => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void ICollection<TValue>.Clear() => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            bool IList.Contains(object value) => Contains(ObservableDictionary<TKey, TValue>.castValue(value));

            public bool Contains(TValue item) => this.parent.Items.ContainsValue(item);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (array.Rank != 1 || array.GetLowerBound(0) != 0)
                    throw new ArgumentException("Unsupported array", nameof(array));
                var a = array as TValue[];
                if (a == null)
                    throw new ArgumentException("Wrong array type", nameof(array));
                if (a.Length - index < Count)
                    throw new ArgumentException("Array size not enough", nameof(array));
                foreach (var item in this)
                {
                    a[index] = item;
                    index++;
                }
            }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex) => ((ICollection)this).CopyTo(array, arrayIndex);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList.IndexOf(object value)
            {
                var v = ObservableDictionary<TKey, TValue>.castValue(value);
                var cmp = EqualityComparer<TValue>.Default;
                for (var i = 0; i < this.parent.Count; i++)
                {
                    if (cmp.Equals(v, this[i]))
                        return i;
                }
                return -1;
            }

            void IList.Insert(int index, object value) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void IList.Remove(object value) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            bool ICollection<TValue>.Remove(TValue item) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void IList.RemoveAt(int index) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");
        }
    }
}
