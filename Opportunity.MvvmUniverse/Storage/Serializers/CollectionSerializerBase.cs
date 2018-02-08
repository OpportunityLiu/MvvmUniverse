using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// If <see cref="IsElementFixedSize"/>: | n | ele1 | ele2 | ... | ele(n-1) |,
    /// othrewise, | n | ele1size | ele1 | ele2size | ele2 | ... | ele(n-1)size | ele(n-1) |.
    /// </summary>
    /// <typeparam name="TElement">Element type</typeparam>
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
            if (this.ElementSerializer.IsFixedSize)
            {
                this.FixedElementSize = this.ElementSerializer.CaculateSize(default);
                this.AlignedFixedElementSize = AlignedSize(this.FixedElementSize);
            }
            this.PrefixSize = AlignedSize(sizeof(int));
        }

        private readonly int alignmentBit;
        protected int PrefixSize { get; }
        public int Alignment => 1 << this.alignmentBit;

        public ISerializer<TElement> ElementSerializer { get; }
        protected bool IsElementFixedSize => this.ElementSerializer.IsFixedSize;
        /// <summary>
        /// Only valid when <see cref="IsElementFixedSize"/> is <see langword="true"/>.
        /// </summary>
        protected int FixedElementSize { get; } = -1;
        /// <summary>
        /// Only valid when <see cref="IsElementFixedSize"/> is <see langword="true"/>.
        /// </summary>
        protected int AlignedFixedElementSize { get; } = -1;

        protected internal int AlignedSize(int size)
        {
            var mask = Alignment - 1;
            if ((size & mask) == 0)
                return size;
            return (size | mask) + 1;
        }

        /// <summary>
        /// Only using when <see cref="IsElementFixedSize"/> is <see langword="false"/>.
        /// </summary>
        protected int CaculateFlexibleElementSize(in TElement value)
        {
            return AlignedSize(this.ElementSerializer.CaculateSize(in value)) + this.PrefixSize;
        }

        /// <summary>
        /// Only using when <see cref="IsElementFixedSize"/> is <see langword="false"/>.
        /// </summary>
        protected void WriteFlexibleElement(in TElement value, ref Span<byte> storage)
        {
            var size = this.ElementSerializer.CaculateSize(in value);
            WriteCount(size, ref storage);
            this.ElementSerializer.Serialize(value, storage.Slice(0, size));
            storage = storage.Slice(AlignedSize(size));
        }
        /// <summary>
        /// Only using when <see cref="IsElementFixedSize"/> is <see langword="false"/>.
        /// </summary>
        protected void ReadFlexibleElement(ref ReadOnlySpan<byte> storage, ref TElement value)
        {
            var size = ReadCount(ref storage);
            this.ElementSerializer.Deserialize(storage.Slice(0, size), ref value);
            storage = storage.Slice(AlignedSize(size));
        }

        /// <summary>
        /// Only using when <see cref="IsElementFixedSize"/> is <see langword="true"/>.
        /// </summary>
        protected void WriteFixedElement(in TElement value, ref Span<byte> storage)
        {
            this.ElementSerializer.Serialize(value, storage.Slice(0, FixedElementSize));
            storage = storage.Slice(AlignedFixedElementSize);
        }
        /// <summary>
        /// Only using when <see cref="IsElementFixedSize"/> is <see langword="true"/>.
        /// </summary>
        protected void ReadFixedElement(ref ReadOnlySpan<byte> storage, ref TElement value)
        {
            this.ElementSerializer.Deserialize(storage.Slice(0, FixedElementSize), ref value);
            storage = storage.Slice(AlignedFixedElementSize);
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

        public bool IsFixedSize => false;
    }
}
