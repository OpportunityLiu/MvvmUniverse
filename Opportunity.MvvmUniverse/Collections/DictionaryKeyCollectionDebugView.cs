using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class DictionaryKeyCollectionDebugView<TKey, TValue>
    {
        private ICollection<TKey> collection;

        public DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                TKey[] items = new TKey[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
