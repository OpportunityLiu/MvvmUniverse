using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// <see cref="ISerializer{T}"/> for SZArray.
    /// </summary>
    /// <typeparam name="TElement">Type of elements of the array.</typeparam>
    public sealed class SZArraySerializer<TElement> : CollectionSerializerBase<TElement>, ISerializer<TElement[]>
    {
        /// <summary>
        /// Create new instance of <see cref="SZArraySerializer{TElement}"/>.
        /// </summary>
        public SZArraySerializer() { }

        /// <summary>
        /// Create new instance of <see cref="SZArraySerializer{TElement}"/>.
        /// </summary>
        /// <param name="elementSerializer"><see cref="ISerializer{T}"/> for elements in the collection.</param>
        public SZArraySerializer(ISerializer<TElement> elementSerializer)
            : base(elementSerializer) { }

        /// <inheritdoc/>
        public void Serialize(in TElement[] value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteInt32(-1);
                return;
            }
            storage.WriteInt32(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                ElementSerializer.Serialize(in value[i], storage);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(DataReader storage, ref TElement[] value)
        {
            var length = storage.ReadInt32();
            if (length < 0)
            {
                value = null;
                return;
            }
            if (value?.Length != length)
            {
                if (TypeTraits.Of<TElement>().Type.IsValueType)
                    value = new TElement[length];
                else
                    Array.Resize(ref value, length);
            }
            for (var i = 0; i < value.Length; i++)
            {
                ElementSerializer.Deserialize(storage, ref value[i]);
            }
        }
    }
}
