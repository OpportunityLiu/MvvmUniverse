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
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableValueCollection : ObservableKeyValueCollectionBase<TValue>, ICollection<TValue>, IReadOnlyList<TValue>, IList
        {
            internal ObservableValueCollection(ObservableDictionary<TKey, TValue> parent) : base(parent) { }

            public TValue this[int index] => this.Parent.ValueItems[index];

            object IList.this[int index]
            {
                get => this[index];
                set => ThrowForReadOnlyCollection(Parent.ToString());
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsFixedSize => true;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool IList.IsReadOnly => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TValue>.IsReadOnly => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.Parent).SyncRoot;

            public List<TValue>.Enumerator GetEnumerator() => this.Parent.ValueItems.GetEnumerator();
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void ForEach(Action<TValue> action) => Parent.ValueItems.ForEach(action);
            public void ForEach(Action<int, TValue> action)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));
                var i = 0;
                foreach (var item in Parent.ValueItems)
                {
                    action(i, item);
                    i++;
                }
            }

            int IList.Add(object value) => ThrowForReadOnlyCollection<int>(Parent.ToString());
            void ICollection<TValue>.Add(TValue item) => ThrowForReadOnlyCollection(Parent.ToString());

            void IList.Clear() => ThrowForReadOnlyCollection(Parent.ToString());
            void ICollection<TValue>.Clear() => ThrowForReadOnlyCollection(Parent.ToString());

            public bool Contains(TValue value) => this.Parent.ContainsValue(value);
            bool IList.Contains(object value) => Contains(CastValue<TValue>(value));

            public void CopyTo(TValue[] array, int arrayIndex) => this.Parent.ValueItems.CopyTo(array, arrayIndex);
            void ICollection.CopyTo(Array array, int index) => ((ICollection)this.Parent.ValueItems).CopyTo(array, index);

            public int IndexOf(TValue value) => this.Parent.ValueItems.IndexOf(value);
            int IList.IndexOf(object value) => IndexOf(CastValue<TValue>(value));

            void IList.Insert(int index, object value) => ThrowForReadOnlyCollection(Parent.ToString());

            void IList.Remove(object value) => ThrowForReadOnlyCollection(Parent.ToString());
            bool ICollection<TValue>.Remove(TValue item) => ThrowForReadOnlyCollection<bool>(Parent.ToString());

            void IList.RemoveAt(int index) => ThrowForReadOnlyCollection(Parent.ToString());
        }
    }
}