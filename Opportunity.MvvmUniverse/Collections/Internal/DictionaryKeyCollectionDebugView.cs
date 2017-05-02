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
                var items = new TKey[this.collection.Count];
                this.collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
