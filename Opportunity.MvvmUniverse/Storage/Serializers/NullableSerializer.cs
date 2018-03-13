using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// <see cref="ISerializer{T}"/> for <see cref="Nullable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Underlying type of <see cref="Nullable{T}"/>.</typeparam>
    public sealed class NullableSerializer<T> : CollectionSerializerBase<T>, ISerializer<T?>
        where T : struct
    {
        /// <summary>
        /// Create new instance of <see cref="NullableSerializer{T}"/>.
        /// </summary>
        public NullableSerializer() { }

        /// <summary>
        /// Create new instance of <see cref="NullableSerializer{T}"/>.
        /// </summary>
        /// <param name="elementSerializer"><see cref="ISerializer{T}"/> for underlying elements.</param>
        public NullableSerializer(ISerializer<T> elementSerializer)
            : base(elementSerializer) { }

        /// <inheritdoc/>
        public void Serialize(in T? value, DataWriter storage)
        {
            if (value is T v)
            {
                storage.WriteBoolean(true);
                ElementSerializer.Serialize(in v, storage);
            }
            else
            {
                storage.WriteBoolean(false);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(DataReader storage, ref T? value)
        {
            if (storage.ReadBoolean())
            {
                var v = value.GetValueOrDefault();
                ElementSerializer.Deserialize(storage, ref v);
                value = v;
            }
            else
                value = null;
        }
    }
}
