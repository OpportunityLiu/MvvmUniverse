using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections.Internal
{
    internal sealed class DictionaryDebugView<K, V>
    {
        private ICollection<KeyValuePair<K, V>> dict;

        public DictionaryDebugView(ICollection<KeyValuePair<K, V>> dictionary)
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
