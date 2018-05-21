using Opportunity.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class EnumSerializer<T> : ISerializer<T>
        where T : struct, Enum, IComparable, IFormattable, IConvertible
    {
        static EnumSerializer()
        {
            if (!TypeTraits.Of<T>().Type.IsEnum)
                throw new InvalidOperationException("Only support enum types.");
        }

        public void Serialize(in T value, DataWriter storage)
        {
            storage.WriteUInt64(((Enum)(object)value).ToUInt64());
        }

        public void Deserialize(DataReader storage, ref T value)
        {
            value = storage.ReadUInt64().ToEnum<T>();
        }
    }
}
