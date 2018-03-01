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
using Windows.UI.Xaml.Interop;
using Windows.Foundation.Collections;
using System.Threading;

namespace Opportunity.MvvmUniverse.Collections
{
    /// <summary>
    /// Read-only view of <see cref="ObservableList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableListView<T> : ObservableCollectionBase<T>, IReadOnlyList<T>, ICollection<T>, IDisposable
    {
        private ObservableList<T> list;

        /// <summary>
        /// <see cref="ObservableList{T}"/> of this view.
        /// </summary>
        protected internal ObservableList<T> List
        {
            get
            {
                var l = this.list;
                if (l == null)
                    throw new InvalidOperationException("Instance disposed.");
                return l;
            }
        }

        /// <summary>
        /// Create a new instance of <see cref="ObservableListView{T}"/>.
        /// </summary>
        /// <param name="list"><see cref="ObservableList{T}"/> of this view.</param>
        public ObservableListView(ObservableList<T> list)
        {
            this.list = list ?? throw new ArgumentNullException(nameof(list));
            list.VectorChanged += this.OnListVectorChanged;
            list.PropertyChanged += this.OnListPropertyChanged;
        }

        protected virtual void OnListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (NeedRaisePropertyChanged)
                OnPropertyChanged(new SinglePropertyChangedEventArgsSource(e.PropertyName));
        }

        protected virtual void OnListVectorChanged(IBindableObservableVector vector, object e)
        {
            if (NeedRaiseVectorChanged)
                OnVectorChanged((IVectorChangedEventArgs)e);
        }

        /// <summary>
        /// Set <see cref="List"/> to <see langword="null"/> and unsubscribe events.
        /// </summary>
        public virtual void Dispose()
        {
            var l = Interlocked.Exchange(ref this.list, null);
            if (l == null)
                return;
            l.VectorChanged -= this.OnListVectorChanged;
            l.PropertyChanged -= this.OnListPropertyChanged;
        }

        /// <inheritdoc />
        public T this[int index] => List[index];

        /// <inheritdoc />
        public int Count => List.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public List<T>.Enumerator GetEnumerator() => List.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => List.GetEnumerator();

        /// <inheritdoc />
        public bool Contains(T item) => List.Contains(item);

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

        bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection(List, false);
        void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(List);
        void ICollection<T>.Clear() => ThrowForReadOnlyCollection(List);
    }

    internal class UndisposableObservableListView<T> : ObservableListView<T>
    {
        public UndisposableObservableListView(ObservableList<T> list)
            : base(list) { }

        public override void Dispose() { }
    }
}
