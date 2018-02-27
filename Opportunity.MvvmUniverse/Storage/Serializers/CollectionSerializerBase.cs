using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// Serializer for collections.
    /// </summary>
    /// <typeparam name="TElement">Element type</typeparam>
    public abstract class CollectionSerializerBase<TElement>
    {
        protected CollectionSerializerBase()
            : this(null) { }

        protected CollectionSerializerBase(ISerializer<TElement> elementSerializer)
        {
            this.ElementSerializer = elementSerializer ?? Serializer<TElement>.Default;
        }

        /// <summary>
        /// <see cref="ISerializer{T}"/> for elements in the collection.
        /// </summary>
        public ISerializer<TElement> ElementSerializer { get; }
    }
}
