using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class NullableSerializer<T> : CollectionSerializerBase<T>, ISerializer<T?>
        where T : struct
    {
        public NullableSerializer() { }

        public NullableSerializer(ISerializer<T> elementSerializer)
            : base(elementSerializer) { }

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
