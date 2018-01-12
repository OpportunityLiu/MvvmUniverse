using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace Opportunity.MvvmUniverse.Collections
{
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public readonly struct RangedListView<T> : IReadOnlyList<T>, ICollection<T>, IList
    {

        public static RangedListView<T> Empty { get; }
            = new RangedListView<T>(Array.Empty<T>(), 0, 0);

        public RangedListView(IReadOnlyList<T> items, ItemIndexRange range)
            : this(items, range.FirstIndex, (int)range.Length)
        {
        }

        public RangedListView(IReadOnlyList<T> items, int startIndex)
            : this(items, startIndex, (items ?? throw new ArgumentNullException(nameof(items))).Count - startIndex) { }

        public RangedListView(IReadOnlyList<T> items, int startIndex, int count)
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
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this.items[this.StartIndex + index];
            }
        }

        public int Count { get; }
        public int StartIndex { get; }

        public ItemIndexRange Range => new ItemIndexRange(StartIndex, (uint)Count);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsReadOnly => true;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized => (this.items as ICollection)?.IsSynchronized ?? false;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot => (this.items as ICollection)?.SyncRoot;

        object IList.this[int index]
        {
            get => this[index];
            set => ThrowForReadOnlyCollection(this.items);
        }

        int IList.Add(object value) => ThrowForReadOnlyCollection(this.items, 0);
        void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(this.items);

        void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(this.items);

        void IList.Clear() => ThrowForReadOnlyCollection(this.items);
        void ICollection<T>.Clear() => ThrowForReadOnlyCollection(this.items);

        public bool Contains(T item) => IndexOf(item) >= 0;
        bool IList.Contains(object value)
        {
            try
            {
                return Contains(CastValue<T>(value));
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public int IndexOf(T item)
        {
            var c = EqualityComparer<T>.Default;
            var ii = 0;
            foreach (var i in this)
            {
                if (c.Equals(i, item))
                    return ii;
                ii++;
            }
            return -1;
        }
        int IList.IndexOf(object value)
        {
            try
            {
                return IndexOf(CastValue<T>(value));
            }
            catch (ArgumentException)
            {
                return -1;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Array size not enough", nameof(array));
            foreach (var item in this)
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1 || array.GetLowerBound(0) != 0)
                throw new ArgumentException("Unsupported array", nameof(array));
            var a = array as T[];
            if (a == null)
                throw new ArgumentException("Wrong array type", nameof(array));
            CopyTo(a, index);
        }

        bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection(this.items, false);
        void IList.Remove(object value) => ThrowForReadOnlyCollection(this.items);
        void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(this.items);

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            internal Enumerator(RangedListView<T> parent)
            {
                this.parent = parent;
                this.currentPosition = parent.StartIndex - 1;
            }

            private RangedListView<T> parent;
            private int currentPosition;

            public T Current => this.parent.items[this.currentPosition];

            object IEnumerator.Current => this.Current;

            void IDisposable.Dispose() { }

            public bool MoveNext()
            {
                var ub = this.parent.StartIndex + this.parent.Count;
                if (this.currentPosition >= ub)
                    return false;
                this.currentPosition++;
                return this.currentPosition < ub;
            }

            public void Reset()
            {
                this.currentPosition = this.parent.StartIndex - 1;
            }
        }
    }

}