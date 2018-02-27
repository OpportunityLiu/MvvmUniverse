using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class CollectionSerializer<TCollection, TElement> : CollectionSerializerBase<TElement>, ISerializer<TCollection>
        where TCollection : ICollection<TElement>
    {
        public CollectionSerializer() { }

        public CollectionSerializer(ISerializer<TElement> elementSerializer)
            : base(elementSerializer) { }

        public void Serialize(in TCollection value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteInt32(-1);
                return;
            }
            storage.WriteInt32(value.Count);
            foreach (var item in value)
            {
                ElementSerializer.Serialize(in item, storage);
            }
        }

        public void Deserialize(DataReader storage, ref TCollection value)
        {
            var length = storage.ReadInt32();
            if (length < 0)
            {
                value = default;
                return;
            }
            if (value == null)
                value = Activator.CreateInstance<TCollection>();
            else
                value.Clear();
            for (var i = 0; i < length; i++)
            {
                var data = default(TElement);
                ElementSerializer.Deserialize(storage, ref data);
                value.Add(data);
            }
        }
    }
}
