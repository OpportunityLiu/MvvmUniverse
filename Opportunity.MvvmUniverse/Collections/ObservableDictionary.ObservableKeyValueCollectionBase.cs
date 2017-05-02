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
        public abstract class ObservableKeyValueCollectionBase : ObservableCollectionBase
        {
            protected ObservableDictionary<TKey, TValue> Parent { get; }

            internal ObservableKeyValueCollectionBase(ObservableDictionary<TKey, TValue> parent)
            {
                if (parent == null)
                    throw new ArgumentNullException(nameof(parent));
                this.Parent = parent;
            }

            internal void RaiseCountChangedInternal()
                => this.RaisePropertyChanged(nameof(Count));

            internal void RaiseCollectionChangedInternal(NotifyCollectionChangedEventArgs e)
                => this.RaiseCollectionChanged(e);

            internal void RaiseCollectionResetInternal()
                => RaiseCollectionReset();

            internal void RaiseCollectionMoveInternal(object item, int newIndex, int oldIndex)
                => RaiseCollectionMove(item, newIndex, oldIndex);

            internal void RaiseCollectionAddInternal(object item, int index)
                => RaiseCollectionAdd(item, index);

            internal void RaiseCollectionRemoveInternal(object item, int index)
                => RaiseCollectionRemove(item, index);

            internal void RaiseCollectionReplaceInternal(object newItem, object oldItem, int index)
                => RaiseCollectionReplace(newItem, oldItem, index);

            public int Count => Parent.Count;

            protected static Exception Modifing() => new InvalidOperationException("This collection is a read only view of ObservableDictionary.");
        }
    }
}
