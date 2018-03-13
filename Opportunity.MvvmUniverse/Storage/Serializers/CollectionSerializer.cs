using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// <see cref="ISerializer{T}"/> for <see cref="ICollection{T}"/>.
    /// </summary>
    /// <typeparam name="TCollection">Type of collection.</typeparam>
    /// <typeparam name="TElement">Type of elements of the array.</typeparam>
    public class CollectionSerializer<TCollection, TElement> : CollectionSerializerBase<TElement>, ISerializer<TCollection>
        where TCollection : ICollection<TElement>
    {
        /// <summary>
        /// Create new instance of <see cref="CollectionSerializer{TCollection, TElement}"/>.
        /// </summary>
        public CollectionSerializer() { }

        /// <summary>
        /// Create new instance of <see cref="CollectionSerializer{TCollection, TElement}"/>.
        /// </summary>
        /// <param name="elementSerializer"><see cref="ISerializer{T}"/> for elements in the collection.</param>
        public CollectionSerializer(ISerializer<TElement> elementSerializer)
            : base(elementSerializer) { }

        /// <summary>
        /// Create new instance of <typeparamref name="TCollection"/>.
        /// </summary>
        /// <returns>New instance of <typeparamref name="TCollection"/>.</returns>
        protected virtual TCollection CreateInstance() => Activator.CreateInstance<TCollection>();

        /// <inheritdoc/>
        public void Serialize(in TCollection value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteInt32(-1);
                return;
            }
            storage.WriteInt32(value.Count);
            foreach (var item in value)
            {
                ElementSerializer.Serialize(in item, storage);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(DataReader storage, ref TCollection value)
        {
            var length = storage.ReadInt32();
            if (length < 0)
            {
                value = default;
                return;
            }
            if (value == null)
                value = CreateInstance();
            else
                value.Clear();
            for (var i = 0; i < length; i++)
            {
                var data = default(TElement);
                ElementSerializer.Deserialize(storage, ref data);
                value.Add(data);
            }
        }
    }
}
