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
        [DebuggerDisplay("Count = {Count}")]
        public abstract class ObservableKeyValueCollectionBase<T> : ObservableCollectionBase<T>
        {
            protected ObservableDictionary<TKey, TValue> Parent { get; }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public int Count => Parent.Count;

            internal ObservableKeyValueCollectionBase(ObservableDictionary<TKey, TValue> parent)
            {
                this.Parent = parent;
            }

            internal void RaiseCountChangedInternal()
                => this.OnPropertyChanged(nameof(Count));

            internal void RaiseCollectionChangedInternal(NotifyCollectionChangedEventArgs e)
                => this.OnCollectionChanged(e);

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
