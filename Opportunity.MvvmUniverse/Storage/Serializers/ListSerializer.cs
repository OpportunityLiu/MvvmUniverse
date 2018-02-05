using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class ListSerializer<TList, TElement> : CollectionSerializerBase<TElement>, ISerializer<TList>
        where TList : IList<TElement>, new()
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
            var count = PrefixSize;
            foreach (var item in value)
            {
                count += CaculateElementSize(in item);
            }
            return count;
        }

        public void Serialize(in TList value, Span<byte> storage)
        {
            if (value == null)
                return;
            WriteCount(value.Count, ref storage);
            foreach (var item in value)
            {
                WriteElement(in item, ref storage);
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
                value = new TList();
            for (var j = value.Count - 1; j >= length; j--)
            {
                value.RemoveAt(j);
            }
            var i = 0;
            foreach (var item in value)
            {
                var data = item;
                ReadElement(ref storage, ref data);
                value[i] = data;
                i++;
            }
            for (; i < length; i++)
            {
                var data = default(TElement);
                ReadElement(ref storage, ref data);
                value.Add(data);
            }
        }
    }
}
