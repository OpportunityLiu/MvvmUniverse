using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class DictionaryValueCollectionDebugView<TKey, TValue>
    {
        private ICollection<TValue> collection;

        public DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                TValue[] items = new TValue[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
