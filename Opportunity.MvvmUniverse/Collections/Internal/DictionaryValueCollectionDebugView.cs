using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections.Internal
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
                var items = new TValue[this.collection.Count];
                this.collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
