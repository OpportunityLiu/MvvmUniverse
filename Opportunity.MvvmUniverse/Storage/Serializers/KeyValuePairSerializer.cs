using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class KeyValuePairSerializer<TKey, TValue> : ISerializer<KeyValuePair<TKey, TValue>>
    {
        public KeyValuePairSerializer() : this(null, null) { }

        public KeyValuePairSerializer(ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer)
        {
            this.KeySerializer = keySerializer ?? Serializer<TKey>.Default;
            this.ValueSerializer = valueSerializer ?? Serializer<TValue>.Default;
        }

        public ISerializer<TKey> KeySerializer { get; }
        public ISerializer<TValue> ValueSerializer { get; }

        public void Serialize(in KeyValuePair<TKey, TValue> value, DataWriter storage)
        {
            KeySerializer.Serialize(value.Key, storage);
            ValueSerializer.Serialize(value.Value, storage);
        }

        public void Deserialize(DataReader storage, ref KeyValuePair<TKey, TValue> value)
        {
            var k = default(TKey);
            var v = default(TValue);
            KeySerializer.Deserialize(storage, ref k);
            ValueSerializer.Deserialize(storage, ref v);
            value = new KeyValuePair<TKey, TValue>(k, v);
        }
    }
}
