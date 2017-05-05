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
    public class ObservableCollectionView<T> : ObservableCollectionBase, IReadOnlyList<T>, IList, ICollection<T>
    {
        protected ObservableCollection<T> Collection { get; }

        public ObservableCollectionView(ObservableCollection<T> collection)
        {
            this.Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            collection.CollectionChanged += this.Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Count));
            RaiseCollectionChanged(e);
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
        bool ICollection.IsSynchronized => ((ICollection)Collection).IsSynchronized;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => ((ICollection)Collection).SyncRoot;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        public List<T>.Enumerator GetEnumerator() => Collection.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Collection).GetEnumerator();

        int IList.Add(object value) => ThrowForReadOnlyCollection<int>(Collection.ToString());

        void IList.Clear() => ThrowForReadOnlyCollection(Collection.ToString());

        bool IList.Contains(object value) => ((IList)Collection).Contains(value);

        void ICollection.CopyTo(Array array, int index) => ((ICollection)Collection).CopyTo(array, index);

        int IList.IndexOf(object value) => ((IList)Collection).IndexOf(value);

        void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(Collection.ToString());

        void IList.Remove(object value) => ThrowForReadOnlyCollection(Collection.ToString());

        void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(Collection.ToString());

        void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(Collection.ToString());

        void ICollection<T>.Clear() => ThrowForReadOnlyCollection(Collection.ToString());

        public bool Contains(T item) => ((ICollection<T>)Collection).Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)Collection).CopyTo(array, arrayIndex);

        bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection<bool>(Collection.ToString());
    }
}
