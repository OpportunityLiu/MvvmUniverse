﻿using Opportunity.MvvmUniverse.Collections.Internal;
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
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableKeyCollection : ObservableKeyValueCollectionBase<TKey>, ICollection<TKey>, IReadOnlyList<TKey>, IList
        {
            internal ObservableKeyCollection(ObservableDictionary<TKey, TValue> parent) : base(parent) { }

            public TKey this[int index] => this.Parent.KeyItems[index];

            object IList.this[int index]
            {
                get => this[index];
                set => ThrowForReadOnlyCollection(Parent);
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
            object ICollection.SyncRoot => ((ICollection)this.Parent).SyncRoot;

            public List<TKey>.Enumerator GetEnumerator() => this.Parent.KeyItems.GetEnumerator();
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void ForEach(Action<TKey> action) => Parent.KeyItems.ForEach(action);
            public void ForEach(Action<int, TKey> action)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));
                var i = 0;
                foreach (var item in Parent.KeyItems)
                {
                    action(i, item);
                    i++;
                }
            }

            int IList.Add(object value) => ThrowForReadOnlyCollection(Parent, 0);
            void ICollection<TKey>.Add(TKey item) => ThrowForReadOnlyCollection(Parent);

            void IList.Clear() => ThrowForReadOnlyCollection(Parent);
            void ICollection<TKey>.Clear() => ThrowForReadOnlyCollection(Parent);

            public bool Contains(TKey key) => this.Parent.ContainsKey(key);
            bool IList.Contains(object value) => ((IDictionary)this.Parent).Contains(value);

            public void CopyTo(TKey[] array, int arrayIndex) => this.Parent.KeyItems.CopyTo(array, arrayIndex);
            void ICollection.CopyTo(Array array, int index) => ((ICollection)this.Parent.KeyItems).CopyTo(array, index);

            public int IndexOf(TKey key)
            {
                if (Parent.KeySet.TryGetValue(key, out var index))
                    return index;
                return -1;
            }
            int IList.IndexOf(object value)
            {
                try
                {
                    return IndexOf(CastKey<TKey>(value));
                }
                catch (ArgumentException)
                {
                    return -1;
                }
            }

            void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(Parent);

            void IList.Remove(object value) => ThrowForReadOnlyCollection(Parent);
            bool ICollection<TKey>.Remove(TKey item) => ThrowForReadOnlyCollection(Parent, false);

            void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(Parent);
        }
    }
}
