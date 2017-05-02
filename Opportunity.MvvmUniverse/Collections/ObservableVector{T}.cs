using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    public class ObservableVector<T>
        : ObservableObject
        , IObservableVector<T>
        , ICollection<T>, ICollection
        , IList<T>, IList
    {
        protected List<T> Items { get; } = new List<T>();

        public ObservableVector() { }

        public event VectorChangedEventHandler<T> VectorChanged;

        protected sealed class VectorChangedEventArgs : IVectorChangedEventArgs
        {
            public VectorChangedEventArgs(CollectionChange collectionChange, uint index)
            {
                this.CollectionChange = collectionChange;
                this.Index = index;
            }

            public VectorChangedEventArgs(CollectionChange collectionChange, int index)
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                this.CollectionChange = collectionChange;
                this.Index = (uint)index;
            }

            public CollectionChange CollectionChange { get; private set; }

            public uint Index { get; internal set; }
        }

        protected void RaiseVectorChanged(VectorChangedEventArgs args)
        {
            var temp = VectorChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                temp.Invoke(this, args);
            });
        }

        protected virtual void InsertItem(int index, T item)
        {
            Items.Insert(index, item);
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemInserted, index));
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void RemoveItem(int index)
        {
            Items.RemoveAt(index);
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemRemoved, index));
            RaisePropertyChanged(nameof(Count));
        }

        protected virtual void SetItem(int index, T item)
        {
            Items[index] = item;
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemChanged, index));
        }

        protected virtual void ClearItems()
        {
            Items.Clear();
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.Reset, 0));
            RaisePropertyChanged(nameof(Count));
        }

        public T this[int index]
        {
            get => Items[index];
            set => SetItem(index, value);
        }

        object IList.this[int index]
        {
            get => Items[index];
            set
            {
                if (!(value is T v))
                    throw new ArgumentException("Wrong type of value.", nameof(value));
                SetItem(index, v);
            }
        }

        public RangedCollectionView<T> GetRangeView(int index, int count)
            => new RangedCollectionView<T>(this.Items, index, count);

        public List<T> GetRange(int index, int count)
            => Items.GetRange(index, count);

        public int Count => Items.Count;

        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)Items).SyncRoot;

        public void Add(T item)
        {
            InsertItem(Items.Count,item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(T item)
            => Items.Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => Items.CopyTo(array, arrayIndex);

        public int IndexOf(T item)
            => Items.IndexOf(item);

        public void Insert(int index, T item)
        {
            InsertItem(index,item);
        }

        public bool Remove(T item)
        {
            var i = IndexOf(item);
            if (i < 0)
                return false;
            RemoveAt(i);
            return true;
        }

        public void RemoveAt(int index)
        {
            RemoveItem(index);
        }

        int IList.Add(object value)
        {
            if (!(value is T v))
                throw new ArgumentException("Wrong type of value.", nameof(value));
            Add(v);
            return Items.Count - 1;
        }

        bool IList.Contains(object value)
            => ((IList)Items).Contains(value);

        void ICollection.CopyTo(Array array, int index)
            => ((ICollection)Items).CopyTo(array, index);

        int IList.IndexOf(object value)
            => ((IList)Items).IndexOf(value);

        void IList.Insert(int index, object value)
        {
            if (!(value is T v))
                throw new ArgumentException("Wrong type of value.", nameof(value));
            Insert(index, v);
        }

        void IList.Remove(object value)
        {
            if (!(value is T v))
                throw new ArgumentException("Wrong type of value.", nameof(value));
            Remove(v);
        }

        public List<T>.Enumerator GetEnumerator()
            => Items.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Items.GetEnumerator();
    }
}
