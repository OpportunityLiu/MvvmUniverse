using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class StructSerializer<T> : ISerializer<T>
        where T : struct
    {
        static StructSerializer()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                throw new InvalidOperationException("Not supported.");
        }

        private static readonly int size = Unsafe.SizeOf<T>();
        public int CaculateSize(in T value) => size;

        public void Deserialize(ReadOnlySpan<byte> storage, ref T value)
        {
            if (storage.IsEmpty)
            {
                value = default;
                return;
            }
            value = storage.NonPortableCast<byte, T>()[0];
        }

        public void Serialize(in T value, Span<byte> storage)
        {
            storage.NonPortableCast<byte, T>()[0] = value;
        }
    }
}
