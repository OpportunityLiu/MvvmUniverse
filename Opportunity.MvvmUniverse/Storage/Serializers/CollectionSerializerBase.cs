using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public abstract class CollectionSerializerBase<TElement>
    {
        protected CollectionSerializerBase()
            : this(null, sizeof(int)) { }

        protected CollectionSerializerBase(ISerializer<TElement> elementSerializer)
            : this(elementSerializer, sizeof(int)) { }

        protected CollectionSerializerBase(ISerializer<TElement> elementSerializer, int alignment)
        {
            if (alignment <= 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), "Must be positive");
            switch (alignment)
            {
            case 1: this.alignmentBit = 0; break;
            case 2: this.alignmentBit = 1; break;
            case 4: this.alignmentBit = 2; break;
            case 8: this.alignmentBit = 3; break;
            default:
                if (alignment < 16)
                    throw new ArgumentOutOfRangeException(nameof(alignment), "Must be power of 2");
                while ((alignment & 1) == 0)
                {
                    alignment >>= 1;
                    this.alignmentBit++;
                }
                if (alignment != 1)
                    throw new ArgumentOutOfRangeException(nameof(alignment), "Must be power of 2");
                break;
            }
            this.ElementSerializer = elementSerializer ?? Serializer<TElement>.Default;
            this.PrefixSize = AlignedSize(sizeof(int));
        }

        private readonly int alignmentBit;
        protected int PrefixSize { get; }
        public int Alignment => 1 << this.alignmentBit;

        public ISerializer<TElement> ElementSerializer { get; }

        protected internal int AlignedSize(int size)
        {
            var mask = Alignment - 1;
            if ((size & mask) == 0)
                return size;
            return (size | mask) + 1;
        }

        protected int CaculateElementSize(in TElement value) => AlignedSize(this.ElementSerializer.CaculateSize(in value)) + this.PrefixSize;
        protected void WriteElement(in TElement value, ref Span<byte> storage)
        {
            var size = this.ElementSerializer.CaculateSize(in value);
            WriteCount(size, ref storage);
            this.ElementSerializer.Serialize(value, storage.Slice(0, size));
            storage = storage.Slice(AlignedSize(size));
        }
        protected void ReadElement(ref ReadOnlySpan<byte> storage, ref TElement value)
        {
            var size = ReadCount(ref storage);
            this.ElementSerializer.Deserialize(storage.Slice(0, size), ref value);
            storage = storage.Slice(AlignedSize(size));
        }

        protected void WriteCount(int value, ref Span<byte> storage)
        {
            storage.NonPortableCast<byte, int>()[0] = value;
            storage = storage.Slice(PrefixSize);
        }
        protected int ReadCount(ref ReadOnlySpan<byte> storage)
        {
            var r = storage.NonPortableCast<byte, int>()[0];
            storage = storage.Slice(PrefixSize);
            return r;
        }
    }
}
