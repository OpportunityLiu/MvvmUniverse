using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class CollectionDebugView<T>
    {
        private ICollection<T> collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] items = new T[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
