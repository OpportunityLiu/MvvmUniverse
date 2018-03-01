﻿using Opportunity.MvvmUniverse.Collections.Internal;
using static Opportunity.MvvmUniverse.Collections.Internal.Helpers;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Specialized;
using Windows.Foundation.Collections;

namespace Opportunity.MvvmUniverse.Collections
{
    public partial class ObservableDictionary<TKey, TValue>
    {
        /// <summary>
        /// Base class for observable collections used in <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="T">type of elements</typeparam>
        [DebuggerDisplay("Count = {Count}")]
        public abstract class ObservableKeyValueCollectionBase<T> : ObservableCollectionBase<T>, ICollection
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

            internal void RaiseVectorChangedInternal(IVectorChangedEventArgs e)
                => OnVectorChanged(e);

            internal void RaiseVectorResetInternal()
                => OnVectorReset();

            internal void RaiseItemInsertedInternal(int index)
                => OnItemInserted(index);

            internal void RaiseItemRemovedInternal(int index)
                => OnItemRemoved(index);

            internal void RaiseItemChangedInternal(int index)
                => OnItemChanged(index);

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized => false;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot => ((ICollection)this.Parent).SyncRoot;
        }
    }
}
