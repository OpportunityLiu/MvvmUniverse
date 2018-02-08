using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class ListSerializer<TList, TElement> : CollectionSerializerBase<TElement>, ISerializer<TList>
        where TList : IList<TElement>
    {
        public ListSerializer()
        {
        }

        public ListSerializer(ISerializer<TElement> elementSerializer) : base(elementSerializer)
        {
        }

        public ListSerializer(ISerializer<TElement> elementSerializer, int alignment) : base(elementSerializer, alignment)
        {
        }

        public int CaculateSize(in TList value)
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

        public void Serialize(in TList value, Span<byte> storage)
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

        public void Deserialize(ReadOnlySpan<byte> storage, ref TList value)
        {
            if (storage.IsEmpty)
            {
                value = default;
                return;
            }
            var length = ReadCount(ref storage);
            if (value == null)
                value = Activator.CreateInstance<TList>();
            for (var j = value.Count - 1; j >= length; j--)
            {
                value.RemoveAt(j);
            }
            var i = 0;
            if (IsElementFixedSize)
            {
                foreach (var item in value)
                {
                    var data = item;
                    ReadFixedElement(ref storage, ref data);
                    value[i] = data;
                    i++;
                }
                for (; i < length; i++)
                {
                    var data = default(TElement);
                    ReadFixedElement(ref storage, ref data);
                    value.Add(data);
                }
            }
            else
            {
                foreach (var item in value)
                {
                    var data = item;
                    ReadFlexibleElement(ref storage, ref data);
                    value[i] = data;
                    i++;
                }
                for (; i < length; i++)
                {
                    var data = default(TElement);
                    ReadFlexibleElement(ref storage, ref data);
                    value.Add(data);
                }
            }
        }
    }
}
