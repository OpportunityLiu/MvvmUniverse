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
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableCollection<T> : ObservableCollectionBase, IList<T>, IReadOnlyList<T>, IList
    {

        protected List<T> Items { get; } = new List<T>();

        public ObservableCollection() { }

        protected virtual void InsertItem(int index, T item)
        {
            Items.Insert(index, item);
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionAdd(item, index);
        }

        protected virtual void RemoveItem(int index)
        {
            var removed = Items[index];
            Items.RemoveAt(index);
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionRemove(removed, index);
        }

        protected virtual void SetItem(int index, T item)
        {
            var old = Items[index];
            Items[index] = item;
            RaiseCollectionReplace(item, old, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
                return;
            var itemToMove = this[oldIndex];
            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, itemToMove);
            RaiseCollectionMove(itemToMove, newIndex, oldIndex);
        }

        protected virtual void ClearItems()
        {
            Items.Clear();
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionReset();
        }

        public int Count => Items.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

        public T this[int index]
        {
            get => Items[index];
            set => SetItem(index, value);
        }

        object IList.this[int index]
        {
            get => Items[index];
            set => SetItem(index, Helpers.CastValue<T>(value));
        }

        public int IndexOf(T item) => Items.IndexOf(item);

        public void Insert(int index, T item) => InsertItem(index, item);

        public void RemoveAt(int index) => RemoveItem(index);

        public void Add(T item) => InsertItem(Items.Count, item);

        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);

        public void Clear() => ClearItems();

        public bool Contains(T item) => Items.Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            var i = Items.IndexOf(item);
            if (i < 0)
                return false;
            RemoveItem(i);
            return true;
        }

        private ObservableCollectionView<T> readOnlyView;

        public ObservableCollectionView<T> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, () => new ObservableCollectionView<T>(this));

        public List<T>.Enumerator GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Items.GetEnumerator();

        int IList.Add(object value)
        {
            Add(Helpers.CastValue<T>(value));
            return Items.Count - 1;
        }

        bool IList.Contains(object value) => Contains(Helpers.CastValue<T>(value));

        int IList.IndexOf(object value) => IndexOf(Helpers.CastValue<T>(value));

        void IList.Insert(int index, object value) => Insert(index, Helpers.CastValue<T>(value));

        void IList.Remove(object value) => Remove(Helpers.CastValue<T>(value));

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);
    }
}
