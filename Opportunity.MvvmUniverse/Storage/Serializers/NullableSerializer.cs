using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class NullableSerializer<T> : ISerializer<T?>
        where T : struct
    {
        private static readonly int valueSize = Unsafe.SizeOf<T>();
        private static readonly int size = valueSize + 1;

        public int CaculateSize(in T? value) => size;

        public bool IsFixedSize => true;

        public void Serialize(in T? value, Span<byte> storage)
        {
            if (value is T v)
            {
                storage.NonPortableCast<byte, T>()[0] = v;
                storage[valueSize] = 1;
            }
            else
            {
                storage[valueSize] = 0;
            }
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref T? value)
        {
            if (storage.IsEmpty || storage[valueSize] == 0)
            {
                value = null;
                return;
            }
            value = storage.NonPortableCast<byte, T>()[0];
        }
    }
}
