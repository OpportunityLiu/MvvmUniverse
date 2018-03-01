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
        /// <summary>
        /// Key collection of <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableKeyCollection : ObservableKeyValueCollectionBase<TKey>, ICollection<TKey>, IReadOnlyList<TKey>
        {
            internal ObservableKeyCollection(ObservableDictionary<TKey, TValue> parent) : base(parent) { }

            /// <inheritdoc/>
            public TKey this[int index] => this.Parent.KeyItems[index];

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TKey>.IsReadOnly => true;

            /// <inheritdoc/>
            public List<TKey>.Enumerator GetEnumerator() => this.Parent.KeyItems.GetEnumerator();
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Iterate all keys in the list.
            /// </summary>
            /// <param name="action">Action for each key.</param>
            public void ForEach(Action<TKey> action) => Parent.KeyItems.ForEach(action);
            /// <summary>
            /// Iterate all keys and their index in the list.
            /// </summary>
            /// <param name="action">Action for each key and its index.</param>
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

            void ICollection<TKey>.Add(TKey item) => ThrowForReadOnlyCollection(Parent);
            void ICollection<TKey>.Clear() => ThrowForReadOnlyCollection(Parent);
            bool ICollection<TKey>.Remove(TKey item) => ThrowForReadOnlyCollection(Parent, false);

            /// <inheritdoc/>
            public bool Contains(TKey key) => this.Parent.ContainsKey(key);

            /// <inheritdoc/>
            public void CopyTo(TKey[] array, int arrayIndex) => this.Parent.KeyItems.CopyTo(array, arrayIndex);

            /// <inheritdoc/>
            public int IndexOf(TKey key)
            {
                if (Parent.KeySet.TryGetValue(key, out var index))
                    return index;
                return -1;
            }

        }
    }
}
