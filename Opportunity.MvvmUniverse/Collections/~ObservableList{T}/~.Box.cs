using System;
using System.Collections;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableList<T>
    {
        private sealed class Box : IReadOnlyList<T>, IList
        {
            public Box(T value) { this.Value = value; }

            public T Value { get; }

            T IReadOnlyList<T>.this[int index] => Value;

            int IReadOnlyCollection<T>.Count => 1;
            int ICollection.Count => 1;
            bool IList.IsFixedSize => true;
            bool IList.IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => Value;
            object IList.this[int index]
            {
                get => Value;
                set => throw new InvalidOperationException();
            }

            IEnumerator<T> getEnumerator()
            {
                yield return Value;
            }

            IEnumerator IEnumerable.GetEnumerator() => getEnumerator();
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => getEnumerator();

            int IList.Add(object value) => throw new InvalidOperationException();
            void IList.Clear() => throw new InvalidOperationException();
            bool IList.Contains(object value) => EqualityComparer<T>.Default.Equals(Value, (T)value);
            int IList.IndexOf(object value) => EqualityComparer<T>.Default.Equals(Value, (T)value) ? 0 : -1;
            void IList.Insert(int index, object value) => throw new InvalidOperationException();
            void IList.Remove(object value) => throw new InvalidOperationException();
            void IList.RemoveAt(int index) => throw new InvalidOperationException();
            void ICollection.CopyTo(Array array, int index) => ((T[])array)[index] = Value;
        }
    }
}
