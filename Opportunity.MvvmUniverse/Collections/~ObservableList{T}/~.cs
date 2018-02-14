using Opportunity.MvvmUniverse.Collections.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class ObservableList<T> : ObservableCollectionBase<T>, IList<T>, IReadOnlyList<T>, IList
    {
        protected List<T> Items { get; }

        public ObservableList() : this(null) { }

        public ObservableList(IEnumerable<T> items)
        {
            if (items == null)
                this.Items = new List<T>();
            else
                this.Items = new List<T>(items);
        }

        protected virtual void InsertItems(int index, IReadOnlyList<T> items)
        {
            if ((items ?? throw new ArgumentNullException(nameof(items))).Count <= 0)
                return;
            if (isSameRef(items))
                items = items.ToList();
            Items.InsertRange(index, items);
            OnPropertyChanged(nameof(Count));
            OnCollectionAdd(items, index);
        }

        protected virtual void RemoveItems(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (count <= 0)
                return;
            if (count == 1)
            {
                var removedItem = Items[index];
                Items.RemoveAt(index);
                OnPropertyChanged(nameof(Count));
                OnCollectionRemove(removedItem, index);
            }
            else
            {
                var removedItems = new T[count];
                Items.CopyTo(index, removedItems, 0, count);
                Items.RemoveRange(index, count);
                OnPropertyChanged(nameof(Count));
                OnCollectionRemove(removedItems, index);
            }
        }

        protected virtual void SetItems(int index, IReadOnlyList<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (index < 0 || index + items.Count > this.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (items.Count <= 0)
                return;
            if (isSameRef(items))
                items = items.ToList();
            if (items.Count == 1)
            {
                var oldItem = Items[index];
                Items[index] = items[0];
                OnCollectionReplace(items[0], oldItem, index);
            }
            else
            {
                var oldItems = new T[items.Count];
                this.Items.CopyTo(index, oldItems, 0, items.Count);
                for (var i = 0; i < items.Count; i++)
                {
                    Items[index + i] = items[i];
                }
                OnCollectionReplace(items, oldItems, index);
            }
        }

        protected virtual void MoveItems(int oldIndex, int newIndex, int count)
        {
            if (oldIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            if (count < 0 || oldIndex + count > Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (newIndex + count > Count || newIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            if (oldIndex == newIndex || count == 0)
                return;
            if (count == 1)
            {
                var itemToMove = this[oldIndex];
                Items.RemoveAt(oldIndex);
                Items.Insert(newIndex, itemToMove);
                OnCollectionMove(itemToMove, newIndex, oldIndex);
            }
            else
            {
                var itemsToMove = new T[count];
                Items.CopyTo(oldIndex, itemsToMove, 0, count);
                Items.RemoveRange(oldIndex, count);
                Items.InsertRange(newIndex, itemsToMove);
                OnCollectionMove(itemsToMove, newIndex, oldIndex);
            }
        }

        protected virtual void ClearItems()
        {
            if (Count == 0)
                return;
            Items.Clear();
            OnPropertyChanged(nameof(Count));
            OnCollectionReset();
        }

        public int Count => Items.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

        public T this[int index]
        {
            get => Items[index];
            set => SetItems(index, new Box(value));
        }
        object IList.this[int index]
        {
            get => Items[index];
            set => this[index] = CastValue<T>(value);
        }

        public void Add(T item) => InsertItems(Items.Count, new Box(item));
        int IList.Add(object value)
        {
            Add(CastValue<T>(value));
            return Items.Count - 1;
        }
        public int AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            var toAdd = (items as IReadOnlyList<T>) ?? items.ToList();
            InsertItems(Count, toAdd);
            return toAdd.Count;
        }

        public void Insert(int index, T item) => InsertItems(index, new Box(item));
        void IList.Insert(int index, object value) => Insert(index, CastValue<T>(value));
        public int InsertRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            var toAdd = (items as IReadOnlyList<T>) ?? items.ToList();
            InsertItems(index, toAdd);
            return toAdd.Count;
        }

        public int SetRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            var toSet = (items as IReadOnlyList<T>) ?? items.ToList();
            SetItems(index, toSet);
            return toSet.Count;
        }

        public void Move(int oldIndex, int newIndex) => MoveItems(oldIndex, newIndex, 1);
        public void MoveRange(int oldIndex, int newIndex, int count) => MoveItems(oldIndex, newIndex, count);

        public bool Remove(T item)
        {
            var i = Items.IndexOf(item);
            if (i < 0)
                return false;
            RemoveItems(i, 1);
            return true;
        }
        void IList.Remove(object value) => Remove(CastValue<T>(value));
        public void RemoveAt(int index) => RemoveItems(index, 1);
        public void RemoveRange(int index, int count) => RemoveItems(index, count);

        public void Clear() => ClearItems();

        public void ForEach(Action<T> action) => Items.ForEach(action);
        public void ForEach(Action<int, T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            var i = 0;
            foreach (var item in Items)
            {
                action(i, item);
                i++;
            }
        }

        public bool Contains(T item) => Items.Contains(item);
        bool IList.Contains(object value) => ((IList)Items).Contains(value);

        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

        private ObservableListView<T> readOnlyView;
        public ObservableListView<T> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, ReadOnlyViewFactory ?? (() => new ObservableListView<T>(this)));

        /// <summary>
        /// This delegate will be called when <see cref="AsReadOnly()"/> first called on this instance.
        /// </summary>
        protected virtual Func<ObservableListView<T>> ReadOnlyViewFactory => () => new ObservableListView<T>(this);

        public int IndexOf(T item) => Items.IndexOf(item);
        int IList.IndexOf(object value) => ((IList)Items).IndexOf(value);

        public List<T>.Enumerator GetEnumerator() => Items.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        private bool isSameRef(object collection)
        {
            if (collection is null)
                return false;
            return ReferenceEquals(collection, this)
                || ReferenceEquals(collection, this.Items)
                || (collection is ObservableListView<T> view && ReferenceEquals(view.List, this));
        }
    }
}
