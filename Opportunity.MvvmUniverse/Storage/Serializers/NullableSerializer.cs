﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class NullableSerializer<T> : ISerializer<T?>
        where T : struct
    {
        private static readonly int size = Unsafe.SizeOf<T>();
        public int CaculateSize(in T? value) => value == null ? 0 : size;

        public void Serialize(in T? value, Span<byte> storage)
        {
            if (value is T v)
                storage.NonPortableCast<byte, T>()[0] = v;
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref T? value)
        {
            if (storage.IsEmpty)
            {
                value = default;
                return;
            }
            value = storage.NonPortableCast<byte, T>()[0];
        }
    }
}
