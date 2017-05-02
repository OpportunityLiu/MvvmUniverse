using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableDictionary<TKey, TValue>
    {
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableKeyCollection : ObservableKeyValueCollectionBase, ICollection<TKey>, IReadOnlyList<TKey>, IList
        {
            internal ObservableKeyCollection(ObservableDictionary<TKey, TValue> parent) : base(parent) { }

            public TKey this[int index] => this.Parent.Items[index].Key;

            object IList.this[int index]
            {
                get => this.Parent.Items[index].Key;
                set => throw Modifing();
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsFixedSize => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TKey>.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.Parent.Items).SyncRoot;

            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var item in this.Parent.Items)
                {
                    yield return item.Key;
                }
            }

            int IList.Add(object value) => throw Modifing();

            void ICollection<TKey>.Add(TKey item) => throw Modifing();

            void IList.Clear() => throw Modifing();

            void ICollection<TKey>.Clear() => throw Modifing();

            bool IList.Contains(object value) => this.Parent.ContainsKey(Helpers.CastKey<TKey>(value));

            public bool Contains(TKey item) => this.Parent.ContainsKey(item);

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                if (array.Rank != 1 || array.GetLowerBound(0) != 0)
                    throw new ArgumentException("Unsupported array", nameof(array));
                var a = array as TKey[];
                if (a == null)
                    throw new ArgumentException("Wrong array type", nameof(array));
                ((ICollection<TKey>)this).CopyTo(a, index);
            }

            void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex)
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
                if (value == null)
                    return -1;
                var k = Helpers.CastKey<TKey>(value);
                if (!this.Parent.KeySet.TryGetValue(k, out var index))
                    return -1;
                return index;
            }

            void IList.Insert(int index, object value) => throw Modifing();

            void IList.Remove(object value) => throw Modifing();
            bool ICollection<TKey>.Remove(TKey item) => throw Modifing();

            void IList.RemoveAt(int index) => throw Modifing();
        }
    }
}
