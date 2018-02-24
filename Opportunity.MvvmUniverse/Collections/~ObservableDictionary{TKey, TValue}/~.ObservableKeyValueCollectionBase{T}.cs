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
        /// Base class for observable collections used in <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="T">type of elements</typeparam>
        [DebuggerDisplay("Count = {Count}")]
        public abstract class ObservableKeyValueCollectionBase<T> : ObservableCollectionBase<T>
        {
            internal ObservableDictionary<TKey, TValue> Parent { get; }

            /// <inheritdoc/>
            public int Count => Parent.Count;

            internal ObservableKeyValueCollectionBase(ObservableDictionary<TKey, TValue> parent)
            {
                this.Parent = parent;
            }

            internal void RaiseCountChangedInternal()
                => this.OnPropertyChanged(nameof(Count));

            internal void RaiseCollectionChangedInternal(NotifyCollectionChangedEventArgs e)
                => OnCollectionChanged(e);

            internal void RaiseCollectionResetInternal()
                => OnCollectionReset();

            internal void RaiseCollectionMoveInternal(T item, int newIndex, int oldIndex)
                => OnCollectionMove(item, newIndex, oldIndex);

            internal void RaiseCollectionAddInternal(T item, int index)
                => OnCollectionAdd(item, index);

            internal void RaiseCollectionRemoveInternal(T item, int index)
                => OnCollectionRemove(item, index);

            internal void RaiseCollectionReplaceInternal(T newItem, T oldItem, int index)
                => OnCollectionReplace(newItem, oldItem, index);
        }
    }
}
