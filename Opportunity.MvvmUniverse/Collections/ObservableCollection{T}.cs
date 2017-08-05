using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Opportunity.MvvmUniverse.Collections
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableCollection<T> : ObservableCollectionBase<T>, IList<T>, IReadOnlyList<T>, IList
    {
        protected List<T> Items { get; }

        public ObservableCollection() : this(null) { }

        public ObservableCollection(IEnumerable<T> items)
        {
            if (items == null)
                this.Items = new List<T>();
            else
                this.Items = new List<T>(items);
        }

        protected virtual void InsertItem(int index, T item)
        {
            Items.Insert(index, item);
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionAdd(item, index);
        }

        protected virtual void InsertItems(int index, IList<T> items)
        {
            if (items == null || items.Count <= 0)
                return;
            Items.InsertRange(index, items);
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionAdd(items, index);
        }

        protected virtual void RemoveItem(int index)
        {
            var removed = Items[index];
            Items.RemoveAt(index);
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionRemove(removed, index);
        }

        protected virtual void RemoveItems(int index, int count)
        {
            if (count <= 0)
                return;
            var removedItems = new T[count];
            Items.CopyTo(index, removedItems, 0, count);
            Items.RemoveRange(index, count);
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionRemove(removedItems, index);
        }

        protected virtual void SetItem(int index, T item)
        {
            var old = Items[index];
            Items[index] = item;
            RaiseCollectionReplace(item, old, index);
        }

        protected virtual void SetItems(int index, IList<T> items)
        {
            if (items == null)
                return;
            var count = items.Count;
            if (count <= 0)
                return;
            if (index + count > Items.Count)
                throw new ArgumentOutOfRangeException(nameof(items), "Too many items.");
            var oldItems = new T[count];
            Items.CopyTo(index, oldItems, 0, count);
            for (var i = 0; i < count; i++)
            {
                Items[index + i] = items[i];
            }
            RaiseCollectionReplace(items, oldItems, index);
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

        protected virtual void MoveItems(int oldIndex, int newIndex, int count)
        {
            if (oldIndex == newIndex || count <= 0)
                return;
            var itemsToMove = new T[count];
            Items.CopyTo(oldIndex, itemsToMove, 0, count);
            Items.RemoveRange(oldIndex, count);
            Items.InsertRange(newIndex, itemsToMove);
            RaiseCollectionMove(itemsToMove, newIndex, oldIndex);
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
            set => this[index] = CastValue<T>(value);
        }

        public void SetRange(int index, IList<T> items) => SetItems(index, items);

        public int IndexOf(T item) => Items.IndexOf(item);

        public void Insert(int index, T item) => InsertItem(index, item);

        public void InsertRange(int index, IList<T> items) => InsertItems(index, items);

        public void RemoveAt(int index) => RemoveItem(index);

        public void RemoveRange(int index, int count) => RemoveItems(index, count);

        public void Add(T item) => InsertItem(Items.Count, item);

        public void AddRange(IList<T> items) => InsertItems(Items.Count, items);

        public void Move(int oldIndex, int newIndex) => MoveItem(oldIndex, newIndex);

        public void MoveRange(int oldIndex, int newIndex, int count) => MoveItems(oldIndex, newIndex, count);

        public void Clear() => ClearItems();

        public bool Contains(T item) => Items.Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            var i = Items.IndexOf(item);
            if (i < 0)
                return false;
            RemoveAt(i);
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
            Add(CastValue<T>(value));
            return Items.Count - 1;
        }

        bool IList.Contains(object value) => Contains(CastValue<T>(value));

        int IList.IndexOf(object value) => IndexOf(CastValue<T>(value));

        void IList.Insert(int index, object value) => Insert(index, CastValue<T>(value));

        void IList.Remove(object value) => Remove(CastValue<T>(value));

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);
    }
}
