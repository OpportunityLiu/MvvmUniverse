using Opportunity.MvvmUniverse.Collections.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Interop;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Read-only view of <see cref="ObservableList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class ObservableListView<T> : ObservableCollectionBase<T>
        , IList<T>, IReadOnlyList<T>, IList
        , ICollection<T>, IReadOnlyCollection<T>, ICollection
        , IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// <see cref="ObservableList{T}"/> of this view.
        /// </summary>
        public ObservableList<T> List { get; }

        /// <summary>
        /// Create a new instance of <see cref="ObservableListView{T}"/>.
        /// </summary>
        /// <param name="list"><see cref="ObservableList{T}"/> of this view.</param>
        public ObservableListView(ObservableList<T> list)
        {
            this.List = list ?? throw new ArgumentNullException(nameof(list));
            list.VectorChanged += WeakDelegate.Create<BindableVectorChangedEventHandler>(this.onListVectorChanged);
            list.PropertyChanged += WeakDelegate.Create<PropertyChangedEventHandler>(this.onListPropertyChanged);
        }

        private void onListPropertyChanged(object _, PropertyChangedEventArgs e)
        {
            OnListPropertyChanged(e);
        }

        /// <summary>
        /// Event handler for <see cref="INotifyPropertyChanged.PropertyChanged"/> of <see cref="List"/>.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnListPropertyChanged(PropertyChangedEventArgs e)
        {
            if (NeedRaisePropertyChanged)
                OnPropertyChanged(e);
        }

        private void onListVectorChanged(IBindableObservableVector _, object e)
        {
            OnListVectorChanged((IVectorChangedEventArgs)e);
        }

        /// <summary>
        /// Event handler for <see cref="IBindableObservableVector.VectorChanged"/> of <see cref="List"/>.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected virtual void OnListVectorChanged(IVectorChangedEventArgs e)
        {
            if (NeedRaiseVectorChanged)
                OnVectorChanged(e);
        }

        /// <inheritdoc />
        public T this[int index] => List[index];
        T IList<T>.this[int index] { get => this[index]; set => ThrowForReadOnlyCollection(List); }

        /// <inheritdoc />
        public int Count => List.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => ((ICollection)List).IsSynchronized;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)List).SyncRoot;

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => List.GetEnumerator();

        /// <inheritdoc />
        public bool Contains(T item) => List.Contains(item);

        /// <inheritdoc />
        public int IndexOf(T item) => List.IndexOf(item);

        /// <summary>
        /// Iterate all items in the list.
        /// </summary>
        /// <param name="action">Action for each item.</param>
        public void ForEach(Action<T> action) => List.ForEach(action);
        /// <summary>
        /// Iterate all items and their index in the list.
        /// </summary>
        /// <param name="action">Action for each item and its index.</param>
        public void ForEach(Action<int, T> action) => List.ForEach(action);

        bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection<bool>(List);
        void IList<T>.RemoveAt(int index) => ThrowForReadOnlyCollection(List);
        void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(List);
        void ICollection<T>.Clear() => ThrowForReadOnlyCollection(List);
        void IList<T>.Insert(int index, T item) => ThrowForReadOnlyCollection(List);
    }
}
