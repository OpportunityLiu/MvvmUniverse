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

        protected sealed class VectorChangedEventArgsCollection : IEnumerable<VectorChangedEventArgs>
        {
            public VectorChangedEventArgsCollection(CollectionChange collectionChange, int index, int count)
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count));
                this.Index = (uint)index;
                this.Count = (uint)count;
                this.current = new VectorChangedEventArgs(collectionChange, this.Index);
            }

            public VectorChangedEventArgsCollection(CollectionChange collectionChange, uint index, uint count)
            {
                this.Index = index;
                this.Count = count;
                this.current = new VectorChangedEventArgs(collectionChange, index);
            }

            public uint Index { get; private set; }
            public uint Count { get; private set; }

            private VectorChangedEventArgs current;

            public IEnumerator<VectorChangedEventArgs> GetEnumerator()
            {
                for (uint i = 0; i < Count; i++)
                {
                    this.current.Index = i + Count;
                    yield return this.current;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
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

        protected void RaiseVectorChanged(IEnumerable<VectorChangedEventArgs> args)
        {
            var temp = VectorChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() =>
            {
                foreach (var item in args)
                {
                    temp.Invoke(this, item);
                }
            });
        }

        public T this[int index]
        {
            get => Items[index];
            set
            {
                if (index < 0 || index >= Items.Count)
                    throw new IndexOutOfRangeException();
                Items[index] = value;
                RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemChanged, index));
            }
        }

        object IList.this[int index]
        {
            get => Items[index];
            set
            {
                if (!(value is T v))
                    throw new ArgumentException("Wrong type of value.", nameof(value));
                this[index] = v;
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
            Items.Add(item);
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemInserted, Items.Count - 1));
            RaisePropertyChanged(nameof(Count));
        }

        public void AddRange(IEnumerable<T> items)
        {
            var oldCount = Items.Count;
            Items.AddRange(items);
            RaiseVectorChanged(new VectorChangedEventArgsCollection(CollectionChange.ItemInserted, oldCount, Items.Count - oldCount));
            RaisePropertyChanged(nameof(Count));
        }

        public void Clear()
        {
            Items.Clear();
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.Reset, 0));
            RaisePropertyChanged(nameof(Count));
        }

        public bool Contains(T item)
            => Items.Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => Items.CopyTo(array, arrayIndex);

        public int IndexOf(T item)
            => Items.IndexOf(item);

        public void Insert(int index, T item)
        {
            Items.Insert(index, item);
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemInserted, index));
            RaisePropertyChanged(nameof(Count));
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            var oldCount = Items.Count;
            Items.InsertRange(index, items);
            RaiseVectorChanged(new VectorChangedEventArgsCollection(CollectionChange.ItemInserted, index, Items.Count - oldCount));
            RaisePropertyChanged(nameof(Count));
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
            Items.RemoveAt(index);
            RaiseVectorChanged(new VectorChangedEventArgs(CollectionChange.ItemRemoved, index));
            RaisePropertyChanged(nameof(Count));
        }

        public void RemoveRange(int index, int count)
        {
            Items.RemoveRange(index, count);
            RaiseVectorChanged(new VectorChangedEventArgsCollection(CollectionChange.ItemRemoved, index, count));
            RaisePropertyChanged(nameof(Count));
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
