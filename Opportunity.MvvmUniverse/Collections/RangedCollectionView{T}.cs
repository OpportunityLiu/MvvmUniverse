using System;
using System.Collections;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    public struct RangedCollectionView<T> : IReadOnlyList<T>, IList
    {
        public static RangedCollectionView<T> Empty => new RangedCollectionView<T>();

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

        bool IList.IsFixedSize => true;

        bool IList.IsReadOnly => true;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => throw new NotImplementedException();

        object IList.this[int index]
        {
            get => this[index];
            set => throw new InvalidOperationException("This view is read-only.");
        }

        public RangedCollectionViewEnumerator GetEnumerator()
        {
            return new RangedCollectionViewEnumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value) => throw new InvalidOperationException("This view is read-only.");

        void IList.Clear() => throw new InvalidOperationException("This view is read-only.");

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

        void IList.Insert(int index, object value) => throw new InvalidOperationException("This view is read-only.");

        void IList.Remove(object value) => throw new InvalidOperationException("This view is read-only.");

        void IList.RemoveAt(int index) => throw new InvalidOperationException("This view is read-only.");

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1 || array.GetLowerBound(0) != 0)
                throw new ArgumentException("Unsupported array", nameof(array));
            var a = array as T[];
            if (a == null)
                throw new ArgumentException("Wrong array type", nameof(array));
            if (a.Length - index < Count)
                throw new ArgumentException("Array size not enough", nameof(array));
            foreach (var item in this)
            {
                a[index] = item;
                index++;
            }
        }

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