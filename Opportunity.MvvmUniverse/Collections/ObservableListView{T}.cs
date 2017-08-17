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
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ObservableListView<T> : ObservableCollectionBase<T>, IReadOnlyList<T>, IList, ICollection<T>
    {
        protected ObservableList<T> Collection { get; }

        public ObservableListView(ObservableList<T> collection)
        {
            this.Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            collection.CollectionChanged += this.Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(e);
        }

        public T this[int index] => Collection[index];
        object IList.this[int index]
        {
            get => ((IList)Collection)[index];
            set => ThrowForReadOnlyCollection(Collection.ToString());
        }

        public int Count => Collection.Count;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => ((IList)Collection).IsFixedSize;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => true;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => ((ICollection)Collection).IsSynchronized;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Collection).SyncRoot;


        int IList.Add(object value) => ThrowForReadOnlyCollection<int>(Collection.ToString());
        void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(Collection.ToString());

        void IList.Clear() => ThrowForReadOnlyCollection(Collection.ToString());

        bool IList.Contains(object value) => ((IList)Collection).Contains(value);

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Collection).CopyTo(array, index);
        public void CopyTo(T[] array, int arrayIndex) => Collection.CopyTo(array, arrayIndex);

        void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(Collection.ToString());

        void IList.Remove(object value) => ThrowForReadOnlyCollection(Collection.ToString());
        void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(Collection.ToString());
        bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection<bool>(Collection.ToString());

        void ICollection<T>.Clear() => ThrowForReadOnlyCollection(Collection.ToString());

        public List<T>.Enumerator GetEnumerator() => Collection.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Collection.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

        int IList.IndexOf(object value) => ((IList)Collection).IndexOf(value);

        public bool Contains(T item) => Collection.Contains(item);

        public void ForEach(Action<T> action) => Collection.ForEach(action);
        public void ForEach(Action<int, T> action) => Collection.ForEach(action);
    }
}
