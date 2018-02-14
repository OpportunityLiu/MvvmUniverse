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
    /// <summary>
    /// Read-only view of <see cref="ObservableList{T}"/>.
    /// </summary>
    /// <typeparam name="T">type of elements</typeparam>
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableListView<T> : ObservableCollectionBase<T>, IReadOnlyList<T>, IList, ICollection<T>
    {
        /// <summary>
        /// <see cref="ObservableList{T}"/> of this view.
        /// </summary>
        protected internal ObservableList<T> List { get; }

        /// <summary>
        /// Create a new instance of <see cref="ObservableListView{T}"/>.
        /// </summary>
        /// <param name="list"><see cref="ObservableList{T}"/> of this view</param>
        public ObservableListView(ObservableList<T> list)
        {
            this.List = list ?? throw new ArgumentNullException(nameof(list));
            list.CollectionChanged += this.Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(e);
        }

        /// <inheritdoc />
        public T this[int index] => List[index];
        object IList.this[int index]
        {
            get => ((IList)List)[index];
            set => ThrowForReadOnlyCollection(List);
        }

        /// <inheritdoc />
        public int Count => List.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => ((IList)List).IsFixedSize;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => true;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => ((ICollection)List).IsSynchronized;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)List).SyncRoot;


        int IList.Add(object value) => ThrowForReadOnlyCollection(List, 0);
        void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(List);

        void IList.Clear() => ThrowForReadOnlyCollection(List);

        bool IList.Contains(object value) => ((IList)List).Contains(value);

        void ICollection.CopyTo(Array array, int index) => ((ICollection)List).CopyTo(array, index);
        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(List);

        void IList.Remove(object value) => ThrowForReadOnlyCollection(List);
        void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(List);
        bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection(List, false);

        void ICollection<T>.Clear() => ThrowForReadOnlyCollection(List);

        /// <inheritdoc />
        public List<T>.Enumerator GetEnumerator() => List.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => List.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

        int IList.IndexOf(object value) => ((IList)List).IndexOf(value);

        /// <inheritdoc />
        public bool Contains(T item) => List.Contains(item);

        /// <inheritdoc />
        public void ForEach(Action<T> action) => List.ForEach(action);
        /// <inheritdoc />
        public void ForEach(Action<int, T> action) => List.ForEach(action);
    }
}
