using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class CollectionSerializer<TCollection, TElement> : CollectionSerializerBase<TElement>, ISerializer<TCollection>
        where TCollection : ICollection<TElement>
    {
        public CollectionSerializer() { }

        public CollectionSerializer(ISerializer<TElement> elementSerializer)
            : base(elementSerializer) { }

        public CollectionSerializer(ISerializer<TElement> elementSerializer, int alignment)
            : base(elementSerializer, alignment) { }

        public int CaculateSize(in TCollection value)
        {
            if (value == null)
                return 0;
            if (IsElementFixedSize)
                return value.Count * AlignedFixedElementSize + PrefixSize;
            var count = PrefixSize;
            foreach (var item in value)
            {
                count += CaculateFlexibleElementSize(in item);
            }
            return count;
        }

        public void Serialize(in TCollection value, Span<byte> storage)
        {
            if (value == null)
                return;
            WriteCount(value.Count, ref storage);
            if (IsElementFixedSize)
                foreach (var item in value)
                {
                    WriteFixedElement(in item, ref storage);
                }
            else
                foreach (var item in value)
                {
                    WriteFlexibleElement(in item, ref storage);
                }
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref TCollection value)
        {
            if (storage.IsEmpty)
            {
                value = default;
                return;
            }
            var length = ReadCount(ref storage);
            if (value == null)
                value = Activator.CreateInstance<TCollection>();
            else
                value.Clear();
            if (IsElementFixedSize)
                for (var i = 0; i < length; i++)
                {
                    var data = default(TElement);
                    ReadFixedElement(ref storage, ref data);
                    value.Add(data);
                }
            else
                for (var i = 0; i < length; i++)
                {
                    var data = default(TElement);
                    ReadFlexibleElement(ref storage, ref data);
                    value.Add(data);
                }
        }
    }
}
