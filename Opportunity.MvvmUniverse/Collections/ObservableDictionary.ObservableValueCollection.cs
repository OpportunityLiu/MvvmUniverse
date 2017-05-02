using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableDictionary<TKey, TValue>
    {
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableValueCollection : ObservableKeyValueCollectionBase, IReadOnlyList<TValue>, ICollection<TValue>, IList
        {
            internal ObservableValueCollection(ObservableDictionary<TKey, TValue> parent) : base(parent) { }

            public TValue this[int index] => this.Parent.Items[index].Value;

            object IList.this[int index]
            {
                get => this.Parent.Items[index].Value;
                set => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsFixedSize => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TValue>.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.Parent.Items).SyncRoot;

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach (var item in this.Parent.Items)
                {
                    yield return item.Value;
                }
            }

            int IList.Add(object value) => Helpers.ThrowForReadOnlyCollection<int>(nameof(ObservableDictionary<TKey,TValue>));

            void ICollection<TValue>.Add(TValue item) => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));

            void IList.Clear() => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));

            void ICollection<TValue>.Clear() => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));

            bool IList.Contains(object value) => Contains(Helpers.CastValue<TValue>(value));

            public bool Contains(TValue item) => this.Parent.ContainsValue(item);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (array.Rank != 1 || array.GetLowerBound(0) != 0)
                    throw new ArgumentException("Unsupported array", nameof(array));
                var a = array as TValue[];
                if (a == null)
                    throw new ArgumentException("Wrong array type", nameof(array));
                ((ICollection<TValue>)this).CopyTo(a, index);
            }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
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

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList.IndexOf(object value)
            {
                var v = Helpers.CastValue<TValue>(value);
                var cmp = EqualityComparer<TValue>.Default;
                for (var i = 0; i < this.Parent.Count; i++)
                {
                    if (cmp.Equals(v, this[i]))
                        return i;
                }
                return -1;
            }

            void IList.Insert(int index, object value) => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));

            void IList.Remove(object value) => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));

            bool ICollection<TValue>.Remove(TValue item) => Helpers.ThrowForReadOnlyCollection<bool>(nameof(ObservableDictionary<TKey,TValue>));

            void IList.RemoveAt(int index) => Helpers.ThrowForReadOnlyCollection(nameof(ObservableDictionary<TKey,TValue>));
        }
    }
}
