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
    /// <summary>
    /// Generic list can notify observers when changes happens.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public partial class ObservableList<T> : ObservableCollectionBase<T>, IList<T>, IReadOnlyList<T>, IList
    {
        /// <summary>
        /// Item storage of the <see cref="ObservableList{T}"/>.
        /// </summary>
        protected List<T> Items { get; }

        /// <summary>
        /// Create instance of <see cref="ObservableList{T}"/>.
        /// </summary>
        public ObservableList() : this(null) { }

        /// <summary>
        /// Create instance of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied to the <see cref="ObservableList{T}"/>.</param>
        public ObservableList(IEnumerable<T> items)
        {
            if (items == null)
                this.Items = new List<T>();
            else
                this.Items = new List<T>(items);
        }

        /// <summary>
        /// Insert items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Start index of items to insert.</param>
        /// <param name="items">Items to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/>.
        /// </exception>
        protected virtual void InsertItems(int index, IReadOnlyList<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (items.Count <= 0)
                return;
            if (isSameRef(items))
                items = items.ToList();
            Items.InsertRange(index, items);
            OnPropertyChanged(nameof(Count));
            OnCollectionAdd(items, index);
        }

        /// <summary>
        /// Remove items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Start index of items to remove.</param>
        /// <param name="count">Count of items to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0,
        /// or <paramref name="count"/> less than 0 or greater than <see cref="Count"/> - <paramref name="index"/>.
        /// </exception>
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

        /// <summary>
        /// Set items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Start index of items to set new value.</param>
        /// <param name="items">New value of items.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0
        /// or greater than <see cref="Count"/> - <paramref name="items"/>.<see cref="IReadOnlyCollection{T}.Count"/>.
        /// </exception>
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

        /// <summary>
        /// Move items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="oldIndex">Old start index of items to move.</param>
        /// <param name="newIndex">New start index of items to move.</param>
        /// <param name="count">Count of items to move.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="oldIndex"/> less than 0,
        /// <paramref name="count"/> less than 0 or greater than <see cref="Count"/> - <paramref name="oldIndex"/>,
        /// or <paramref name="newIndex"/> less than 0 or greater than <see cref="Count"/> - <paramref name="count"/>.
        /// </exception>
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

        /// <summary>
        /// Remove all items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        protected virtual void ClearItems()
        {
            if (Count == 0)
                return;
            Items.Clear();
            OnPropertyChanged(nameof(Count));
            OnCollectionReset();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <summary>
        /// Add item at the end of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item) => InsertItems(Items.Count, new Box(item));
        int IList.Add(object value)
        {
            Add(CastValue<T>(value));
            return Items.Count - 1;
        }
        /// <summary>
        /// Add items at the end of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="items">Items to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        /// <returns>Count of items added.</returns>
        public int AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            var toAdd = (items as IReadOnlyList<T>) ?? items.ToList();
            InsertItems(Count, toAdd);
            return toAdd.Count;
        }

        /// <summary>
        /// Insert item in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Index of item to insert.</param>
        /// <param name="item">Item to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/>.
        /// </exception>
        public void Insert(int index, T item) => InsertItems(index, new Box(item));
        void IList.Insert(int index, object value) => Insert(index, CastValue<T>(value));
        /// <summary>
        /// Insert items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Start index of items to insert.</param>
        /// <param name="items">Items to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/>.
        /// </exception>
        /// <returns>Count of items inserted.</returns>
        public int InsertRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            var toAdd = (items as IReadOnlyList<T>) ?? items.ToList();
            InsertItems(index, toAdd);
            return toAdd.Count;
        }

        /// <summary>
        /// Set items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Start index of items to set new value.</param>
        /// <param name="items">New value of items.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0
        /// or greater than <see cref="Count"/> - <paramref name="items"/>.<see cref="IReadOnlyCollection{T}.Count"/>.
        /// </exception>
        /// <returns>Count of items changed.</returns>
        public int SetRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            var toSet = (items as IReadOnlyList<T>) ?? items.ToList();
            SetItems(index, toSet);
            return toSet.Count;
        }

        /// <summary>
        /// Move item in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="oldIndex">Old index of item to move.</param>
        /// <param name="newIndex">New index of item to move.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="oldIndex"/> less than 0 or greater than <see cref="Count"/> - 1,
        /// or <paramref name="newIndex"/> less than 0 or greater than <see cref="Count"/> - 1.
        /// </exception>
        public void Move(int oldIndex, int newIndex) => MoveItems(oldIndex, newIndex, 1);
        /// <summary>
        /// Move items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="oldIndex">Old start index of items to move.</param>
        /// <param name="newIndex">New start index of items to move.</param>
        /// <param name="count">Count of items to move.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="oldIndex"/> less than 0,
        /// <paramref name="count"/> less than 0 or greater than <see cref="Count"/> - <paramref name="oldIndex"/>,
        /// or <paramref name="newIndex"/> less than 0 or greater than <see cref="Count"/> - <paramref name="count"/>.
        /// </exception>
        public void MoveRange(int oldIndex, int newIndex, int count) => MoveItems(oldIndex, newIndex, count);

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var i = Items.IndexOf(item);
            if (i < 0)
                return false;
            RemoveItems(i, 1);
            return true;
        }
        void IList.Remove(object value) => Remove(CastValue<T>(value));
        /// <inheritdoc/>
        public void RemoveAt(int index) => RemoveItems(index, 1);
        /// <summary>
        /// Remove items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Start index of items to remove.</param>
        /// <param name="count">Count of items to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0,
        /// or <paramref name="count"/> less than 0 or greater than <see cref="Count"/> - <paramref name="index"/>.
        /// </exception>
        public void RemoveRange(int index, int count) => RemoveItems(index, count);

        /// <summary>
        /// Remove all items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        public void Clear() => ClearItems();

        /// <summary>
        /// Iterate all items in the list.
        /// </summary>
        /// <param name="action">Action for each item.</param>
        public void ForEach(Action<T> action) => Items.ForEach(action);
        /// <summary>
        /// Iterate all items and their index in the list.
        /// </summary>
        /// <param name="action">Action for each item and its index.</param>
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

        /// <inheritdoc/>
        public bool Contains(T item) => Items.Contains(item);
        bool IList.Contains(object value) => ((IList)Items).Contains(value);

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)Items).CopyTo(array, index);

        private ObservableListView<T> readOnlyView;
        /// <summary>
        /// Get a read-only view of current instance.
        /// </summary>
        /// <returns>A read-only view of current instance.</returns>
        public ObservableListView<T> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, ReadOnlyViewFactory);

        /// <summary>
        /// This method will be called when <see cref="AsReadOnly()"/> first called on this instance.
        /// </summary>
        protected virtual ObservableListView<T> ReadOnlyViewFactory() => new ObservableListView<T>(this);

        /// <inheritdoc/>
        public int IndexOf(T item) => Items.IndexOf(item);
        int IList.IndexOf(object value) => ((IList)Items).IndexOf(value);

        /// <inheritdoc/>
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
