using Opportunity.MvvmUniverse.Collections.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Interop;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Generic list can notify observers when changes happens.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public partial class ObservableList<T> : ObservableCollectionBase<T>
        , IList<T>, IReadOnlyList<T>
        , ICollection<T>, IReadOnlyCollection<T>, ICollection
        , IEnumerable<T>
    {
        /// <summary>
        /// Item storage of the <see cref="ObservableList{T}"/>.
        /// </summary>
        protected IList<T> Items { get; }

        /// <summary>
        /// Create instance of <see cref="ObservableList{T}"/>.
        /// </summary>
        public ObservableList() : this(null) { }

        /// <summary>
        /// Create instance of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied to the <see cref="ObservableList{T}"/>.</param>
        public ObservableList(IEnumerable<T> items)
            : this(new List<T>(items.EmptyIfNull()), false) { }

        /// <summary>
        /// Create instance of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="items">Items will be copied or wrapped to the <see cref="ObservableList{T}"/>.</param>
        /// <param name="makeCopy">Indicates the <paramref name="items"/> should be copied or directly set to <see cref="Items"/>.</param>
        protected ObservableList(IList<T> items, bool makeCopy)
        {
            if (makeCopy)
                Items = items is null ? new List<T>() : new List<T>(items);
            else
                Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Create instance of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="list">The list will be wrapped to the <see cref="ObservableList{T}"/>, as the <see cref="Items"/>.</param>
        public static ObservableList<T> CreaterWrapper(IList<T> list) => new ObservableList<T>(list, false);

        /// <summary>
        /// Insert item to the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Index of item to insert.</param>
        /// <param name="item">Item to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/>.
        /// </exception>
        protected virtual void InsertItem(int index, T item)
        {
            Items.Insert(index, item);
            OnPropertyChanged(ConstPropertyChangedEventArgs.Count);
            OnItemInserted(index);
        }

        /// <summary>
        /// Remove item of the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/> - 1.
        /// </exception>
        protected virtual void RemoveItem(int index)
        {
            Items.RemoveAt(index);
            OnPropertyChanged(ConstPropertyChangedEventArgs.Count);
            OnItemRemoved(index);
        }

        /// <summary>
        /// Set item of the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Index of item to set new value.</param>
        /// <param name="item">New value of item.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/> - 1.
        /// </exception>
        protected virtual void SetItem(int index, T item)
        {
            var old = Items[index];
            if (default(T) == null && ReferenceEquals(old, item))
                return;
            Items[index] = item;
            OnItemChanged(index);
        }

        /// <summary>
        /// Remove all items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        protected virtual void ClearItems()
        {
            if (Count == 0)
                return;
            Items.Clear();
            OnPropertyChanged(ConstPropertyChangedEventArgs.Count);
            OnVectorReset();
        }

        /// <inheritdoc/>
        public int Count => Items.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => false;

        /// <inheritdoc/>
        public T this[int index]
        {
            get => Items[index];
            set => SetItem(index, value);
        }

        /// <summary>
        /// Add item at the end of <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item) => InsertItem(Items.Count, item);

        /// <summary>
        /// Insert item to the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Index of item to insert.</param>
        /// <param name="item">Item to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/>.
        /// </exception>
        public void Insert(int index, T item) => InsertItem(index, item);

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var i = Items.IndexOf(item);
            if (i < 0)
                return false;
            RemoveItem(i);
            return true;
        }

        /// <summary>
        /// Remove item of the <see cref="ObservableList{T}"/>.
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> less than 0 or greater than <see cref="Count"/> - 1.
        /// </exception>
        public void RemoveAt(int index) => RemoveItem(index);

        /// <summary>
        /// Remove all items in the <see cref="ObservableList{T}"/>.
        /// </summary>
        public void Clear() => ClearItems();

        /// <summary>
        /// Iterate all items in the list.
        /// </summary>
        /// <param name="action">Action for each item.</param>
        public void ForEach(Action<T> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            foreach (var item in Items)
            {
                action(item);
            }
        }
        /// <summary>
        /// Iterate all items and their index in the list.
        /// </summary>
        /// <param name="action">Action for each item and its index.</param>
        public void ForEach(Action<int, T> action)
        {
            if (action is null)
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

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

        private ObservableListView<T> readOnlyView;
        /// <summary>
        /// Get a read-only view of current instance.
        /// </summary>
        /// <returns>A read-only view of current instance.</returns>
        public ObservableListView<T> AsReadOnly()
            => LazyInitializer.EnsureInitialized(ref this.readOnlyView, CreateReadOnlyView);

        /// <summary>
        /// This method will be called when <see cref="AsReadOnly()"/> first called on this instance.
        /// </summary>
        protected virtual ObservableListView<T> CreateReadOnlyView()
            => new ObservableListView<T>(this);

        /// <inheritdoc/>
        public int IndexOf(T item) => Items.IndexOf(item);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
    }
}
