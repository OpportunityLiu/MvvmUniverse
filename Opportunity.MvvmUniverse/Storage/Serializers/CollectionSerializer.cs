using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class CollectionSerializer<TCollection, TElement> : ISerializer<TCollection>
        where TCollection : ICollection<TElement>, new()
    {
        private readonly SZArraySerializer<TElement> inner = new SZArraySerializer<TElement>();

        public ISerializer<TElement> ElementSerializer
        {
            get => this.inner.ElementSerializer;
            set => this.inner.ElementSerializer = value;
        }

        public TCollection Deserialize(object value)
        {
            var r = this.inner.Deserialize(value);
            if (r == null)
                return default;
            var collection = new TCollection();
            foreach (var item in r)
            {
                collection.Add(item);
            }
            return collection;
        }

        public object Serialize(TCollection value)
        {
            if (value == null)
                return this.inner.Serialize(null);
            if (value.Count == 0)
                return this.inner.Serialize(Array.Empty<TElement>());
            return this.inner.Serialize(value.ToArray());
        }
    }
}
