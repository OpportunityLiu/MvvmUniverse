using System.Collections.Generic;
using System.Diagnostics;

namespace Opportunity.MvvmUniverse.Collections.Internal
{
    internal sealed class DictionaryDebugView<TKey, TValue>
    {
        private readonly ICollection<KeyValuePair<TKey, TValue>> dict;

        public DictionaryDebugView(ICollection<KeyValuePair<TKey, TValue>> dictionary)
        {
            this.dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items
        {
            get
            {
                var items = new KeyValuePair<TKey, TValue>[this.dict.Count];
                this.dict.CopyTo(items, 0);
                return items;
            }
        }
    }
}
