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
    /// <see cref="ISerializer{T}"/> for <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="TList">Type of list.</typeparam>
    /// <typeparam name="TElement">Type of elements of the array.</typeparam>
    public class ListSerializer<TList, TElement> : CollectionSerializerBase<TElement>, ISerializer<TList>
        where TList : IList<TElement>
    {
        /// <summary>
        /// Create new instance of <see cref="ListSerializer{TList, TElement}"/>.
        /// </summary>
        public ListSerializer() { }

        /// <summary>
        /// Create new instance of <see cref="ListSerializer{TList, TElement}"/>.
        /// </summary>
        /// <param name="elementSerializer"><see cref="ISerializer{T}"/> for elements in the collection.</param>
        public ListSerializer(ISerializer<TElement> elementSerializer)
            : base(elementSerializer) { }

        /// <summary>
        /// Create new instance of <typeparamref name="TList"/>.
        /// </summary>
        /// <returns>New instance of <typeparamref name="TList"/>.</returns>
        protected virtual TList CreateInstance() => Activator.CreateInstance<TList>();

        /// <inheritdoc/>
        public void Serialize(in TList value, DataWriter storage)
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
        public void Deserialize(DataReader storage, ref TList value)
        {
            var length = storage.ReadInt32();
            if (length < 0)
            {
                value = default;
                return;
            }
            if (value == null)
                value = CreateInstance();
            for (var j = value.Count - 1; j >= length; j--)
            {
                value.RemoveAt(j);
            }
            var i = 0;
            for (; i < value.Count; i++)
            {
                var data = value[i];
                ElementSerializer.Deserialize(storage, ref data);
                value[i] = data;
            }
            for (; i < length; i++)
            {
                var data = default(TElement);
                ElementSerializer.Deserialize(storage, ref data);
                value.Add(data);
            }
        }
    }
}
