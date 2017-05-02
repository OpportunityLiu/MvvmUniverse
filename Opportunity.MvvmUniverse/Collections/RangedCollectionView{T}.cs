using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public struct RangedCollectionView<T> : IReadOnlyList<T>, ICollection<T>, IList
    {
        public static RangedCollectionView<T> Empty { get; }
            = new RangedCollectionView<T>(Array.Empty<T>(), 0, 0);

        public RangedCollectionView(IReadOnlyList<T> items, int startIndex, int count)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (unchecked((uint)startIndex > (uint)items.Count))
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0 || startIndex + count > items.Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            this.items = items;
            this.StartIndex = startIndex;
            this.Count = count;
        }

        private readonly IReadOnlyList<T> items;

        public T this[int index]
        {
            get
            {
                if (unchecked((uint)index >= (uint)this.Count))
                    throw new IndexOutOfRangeException();
                return this.items[this.StartIndex + index];
            }
        }

        public int Count { get; }
        public int StartIndex { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => throw new NotImplementedException();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        object IList.this[int index]
        {
            get => this[index];
            set => Helpers.ThrowForReadOnlyCollection(this.items.ToString());
        }

        public RangedCollectionViewEnumerator GetEnumerator()
        {
            return new RangedCollectionViewEnumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value) => Helpers.ThrowForReadOnlyCollection<int>(this.items.ToString());

        void IList.Clear() => Helpers.ThrowForReadOnlyCollection(this.items.ToString());

        bool IList.Contains(object value)
        {
            var i = ((IList)this).IndexOf(value);
            return i != -1;
        }

        int IList.IndexOf(object value)
        {
            var v = Helpers.CastValue<T>(value);
            var c = EqualityComparer<T>.Default;
            for (var i = 0; i < Count; i++)
            {
                if (c.Equals(v, this[i]))
                    return i;
            }
            return -1;
        }

        void IList.Insert(int index, object value) => Helpers.ThrowForReadOnlyCollection(this.items.ToString());

        void IList.Remove(object value) => Helpers.ThrowForReadOnlyCollection(this.items.ToString());

        void IList.RemoveAt(int index) => Helpers.ThrowForReadOnlyCollection(this.items.ToString());

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1 || array.GetLowerBound(0) != 0)
                throw new ArgumentException("Unsupported array", nameof(array));
            var a = array as T[];
            if (a == null)
                throw new ArgumentException("Wrong array type", nameof(array));
            ((ICollection<T>)this).CopyTo(a, index);
        }

        void ICollection<T>.Add(T item) => Helpers.ThrowForReadOnlyCollection(this.items.ToString());

        void ICollection<T>.Clear() => Helpers.ThrowForReadOnlyCollection(this.items.ToString());

        bool ICollection<T>.Contains(T item)
        {
            var c = EqualityComparer<T>.Default;
            foreach (var i in this)
            {
                if (c.Equals(i, item))
                    return true;
            }
            return false;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentException("Wrong array type", nameof(array));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Array size not enough", nameof(array));
            foreach (var item in this)
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        bool ICollection<T>.Remove(T item) => Helpers.ThrowForReadOnlyCollection<bool>(this.items.ToString());

        public struct RangedCollectionViewEnumerator : IEnumerator<T>
        {
            internal RangedCollectionViewEnumerator(RangedCollectionView<T> parent)
            {
                this.parent = parent;
                this.currentPosition = parent.StartIndex - 1;
            }

            private RangedCollectionView<T> parent;
            private int currentPosition;

            public T Current => this.parent.items[this.currentPosition];

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                this.currentPosition++;
                return this.currentPosition < this.parent.StartIndex + this.parent.Count;
            }

            public void Reset()
            {
                this.currentPosition = this.parent.StartIndex - 1;
            }
        }
    }

}