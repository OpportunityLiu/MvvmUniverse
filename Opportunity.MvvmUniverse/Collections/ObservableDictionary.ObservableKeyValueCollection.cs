using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableDictionary<TKey, TValue>
    {
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableKeyValueCollection<T> : ObservableCollectionBase<T>, ICollection<T>, IReadOnlyList<T>, IList
        {
            private readonly bool isKey;
            private readonly ObservableDictionary<TKey, TValue> parent;

            private List<T> Collection
                => (List<T>)(this.isKey ? (object)this.parent.KeyItems : this.parent.valueItems);

            internal ObservableKeyValueCollection(ObservableDictionary<TKey, TValue> parent, bool isKey)
            {
                this.parent = parent;
                this.isKey = isKey;
            }

            internal void RaiseCountChangedInternal()
                => this.RaisePropertyChanged(nameof(Count));

            internal void RaiseCollectionChangedInternal(NotifyCollectionChangedEventArgs e)
                => this.RaiseCollectionChanged(e);

            internal void RaiseCollectionResetInternal()
                => RaiseCollectionReset();

            internal void RaiseCollectionMoveInternal(T item, int newIndex, int oldIndex)
                => RaiseCollectionMove(item, newIndex, oldIndex);

            internal void RaiseCollectionAddInternal(T item, int index)
                => RaiseCollectionAdd(item, index);

            internal void RaiseCollectionRemoveInternal(T item, int index)
                => RaiseCollectionRemove(item, index);

            internal void RaiseCollectionReplaceInternal(T newItem, T oldItem, int index)
                => RaiseCollectionReplace(newItem, oldItem, index);

            public T this[int index] => this.Collection[index];

            object IList.this[int index]
            {
                get => this.Collection[index];
                set => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));
            }

            public int Count => this.parent.Count;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsFixedSize => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<T>.IsReadOnly => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.parent.KeySet).SyncRoot;

            public List<T>.Enumerator GetEnumerator() => this.Collection.GetEnumerator();

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList.Add(object value) => ThrowForReadOnlyCollection<int>(nameof(ObservableDictionary<T, TValue>));

            void ICollection<T>.Add(T item) => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));

            void IList.Clear() => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));

            void ICollection<T>.Clear() => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));

            bool IList.Contains(object value)
                => this.isKey
                ? this.parent.ContainsKey(CastKey<TKey>(value))
                : this.parent.ContainsValue(CastValue<TValue>(value));

            public bool Contains(T item)
                => this.isKey
                ? ((Dictionary<T, int>)(object)this.parent.KeySet).ContainsKey(item)
                : this.Collection.Contains(item);

            void ICollection.CopyTo(Array array, int index) => ((ICollection)this.Collection).CopyTo(array, index);

            void ICollection<T>.CopyTo(T[] array, int arrayIndex) => this.Collection.CopyTo(array, arrayIndex);

            int IList.IndexOf(object value) => ((IList)this.Collection).IndexOf(value);

            void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));

            void IList.Remove(object value) => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));
            bool ICollection<T>.Remove(T item) => ThrowForReadOnlyCollection<bool>(nameof(ObservableDictionary<T, TValue>));

            void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(nameof(ObservableDictionary<T, TValue>));
        }
    }
}
