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
        /// Value collection of <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        public sealed class ObservableValueCollection : ObservableKeyValueCollectionBase<TValue>
            , IReadOnlyList<TValue>
            , ICollection<TValue>, IReadOnlyCollection<TValue>
            , IEnumerable<TValue>
        {
            internal ObservableValueCollection(ObservableDictionary<TKey, TValue> parent) : base(parent) { }

            /// <inheritdoc/>
            public TValue this[int index] => this.Parent.ValueItems[index];

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection<TValue>.IsReadOnly => false;

            /// <inheritdoc/>
            public List<TValue>.Enumerator GetEnumerator() => this.Parent.ValueItems.GetEnumerator();
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Iterate all values in the list.
            /// </summary>
            /// <param name="action">Action for each value.</param>
            public void ForEach(Action<TValue> action) => Parent.ValueItems.ForEach(action);
            /// <summary>
            /// Iterate all values and their index in the list.
            /// </summary>
            /// <param name="action">Action for each value and its index.</param>
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

            void ICollection<TValue>.Add(TValue item) => ThrowForReadOnlyCollection(Parent);
            void ICollection<TValue>.Clear() => ThrowForReadOnlyCollection(Parent);
            bool ICollection<TValue>.Remove(TValue item) => ThrowForReadOnlyCollection<bool>(Parent);

            /// <inheritdoc/>
            public bool Contains(TValue value) => this.Parent.ContainsValue(value);

            /// <inheritdoc/>
            public void CopyTo(TValue[] array, int arrayIndex) => this.Parent.ValueItems.CopyTo(array, arrayIndex);

            /// <inheritdoc/>
            public int IndexOf(TValue value) => this.Parent.ValueItems.IndexOf(value);

        }
    }
}
