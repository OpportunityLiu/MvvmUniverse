using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class Mscorlib_DictionaryKeyCollectionDebugView<TKey, TValue>
    {
        private ICollection<TKey> collection;

        public Mscorlib_DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                TKey[] items = new TKey[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    public partial class ObservableDictionary<TKey, TValue> : ObservableCollectionBase, IDictionary<TKey, TValue>, IDictionary, IList<KeyValuePair<TKey, TValue>>, IReadOnlyList<KeyValuePair<TKey, TValue>>, IList
    {
        [DebuggerTypeProxy(typeof(Mscorlib_DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableKeyCollection : ObservableCollectionBase, ICollection<TKey>, IReadOnlyList<TKey>, IList
        {
            private readonly ObservableDictionary<TKey, TValue> parent;

            public ObservableKeyCollection(ObservableDictionary<TKey, TValue> parent)
            {
                if (parent == null)
                    throw new ArgumentNullException(nameof(parent));
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
                    RaiseCollectionAdd(newItem.Key, e.NewStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    RaiseCollectionMove(newItem.Key, e.NewStartingIndex, e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RaiseCollectionRemove(oldItem.Key, e.OldStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    RaiseCollectionReplace(newItem.Key, oldItem.Key, e.NewStartingIndex);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RaiseCollectionReset();
                    break;
                }
            }

            public TKey this[int index] => this.parent.SortedKeys[index];

            object IList.this[int index]
            {
                get => this.parent.SortedKeys[index];
                set => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");
            }

            public int Count => this.parent.Count;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsFixedSize => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TKey>.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.parent.Items).SyncRoot;

            public IEnumerator<TKey> GetEnumerator() => this.parent.SortedKeys.GetEnumerator();

            int IList.Add(object value) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void ICollection<TKey>.Add(TKey item) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void IList.Clear() => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void ICollection<TKey>.Clear() => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            bool IList.Contains(object value) => this.parent.ContainsKey(ObservableDictionary<TKey, TValue>.castKey(value));

            public bool Contains(TKey item) => this.parent.ContainsKey(item);

            void ICollection.CopyTo(Array array, int index) => ((ICollection)this.parent.SortedKeys).CopyTo(array, index);

            void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex) => ((ICollection<TKey>)this.parent.SortedKeys).CopyTo(array, arrayIndex);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList.IndexOf(object value) => this.parent.indexOf(ObservableDictionary<TKey, TValue>.castKey(value));

            void IList.Insert(int index, object value) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void IList.Remove(object value) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            bool ICollection<TKey>.Remove(TKey item) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");

            void IList.RemoveAt(int index) => throw new InvalidOperationException("This collection is a read only view of ObservableDictionary.");
        }
    }
}
