using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class SZArraySerializer<TElement> : CollectionSerializerBase<TElement>, ISerializer<TElement[]>
    {
        public SZArraySerializer()
        {
        }

        public SZArraySerializer(ISerializer<TElement> elementSerializer) : base(elementSerializer)
        {
        }

        public SZArraySerializer(ISerializer<TElement> elementSerializer, int alignment) : base(elementSerializer, alignment)
        {
        }

        public int CaculateSize(in TElement[] value)
        {
            if (value == null)
                return 0;
            var count = PrefixSize;
            for (var i = 0; i < value.Length; i++)
            {
                count += CaculateElementSize(in value[i]);
            }
            return count;
        }

        public void Serialize(in TElement[] value, Span<byte> storage)
        {
            if (value == null)
                return;
            WriteCount(value.Length, ref storage);
            for (var i = 0; i < value.Length; i++)
            {
                WriteElement(in value[i], ref storage);
            }
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref TElement[] value)
        {
            if (storage.IsEmpty)
            {
                value = null;
                return;
            }
            var length = ReadCount(ref storage);
            if (value?.Length != length)
            {
                if (isElementValueType)
                    value = new TElement[length];
                else
                    Array.Resize(ref value, length);
            }
            for (var i = 0; i < value.Length; i++)
            {
                ReadElement(ref storage, ref value[i]);
            }
        }

        private static readonly bool isElementValueType = TypeTraits.Of<TElement>().Type.IsValueType;
    }
}
