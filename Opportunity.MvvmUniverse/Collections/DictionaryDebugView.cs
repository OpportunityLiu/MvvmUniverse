using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections
{
    internal sealed class DictionaryDebugView<K, V>
    {
        private IDictionary<K, V> dict;

        public DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            this.dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                var items = new KeyValuePair<K, V>[this.dict.Count];
                this.dict.CopyTo(items, 0);
                return items;
            }
        }
    }
}
