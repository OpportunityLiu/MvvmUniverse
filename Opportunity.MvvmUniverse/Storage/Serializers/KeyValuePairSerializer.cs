using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class KeyValuePairSerializer<TKey, TValue> : ISerializer<KeyValuePair<TKey, TValue>>
    {
        private const int offset = sizeof(int);

        public KeyValuePairSerializer() : this(null, null) { }

        public KeyValuePairSerializer(ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer)
        {
            this.KeySerializer = keySerializer ?? Serializer<TKey>.Default;
            this.ValueSerializer = valueSerializer ?? Serializer<TValue>.Default;
        }

        public ISerializer<TKey> KeySerializer { get; }
        public ISerializer<TValue> ValueSerializer { get; }

        public bool IsFixedSize => KeySerializer.IsFixedSize && ValueSerializer.IsFixedSize;

        public int CaculateSize(in KeyValuePair<TKey, TValue> value)
        {
            if (IsFixedSize)
                return KeySerializer.CaculateSize(value.Key) + ValueSerializer.CaculateSize(value.Value);
            return KeySerializer.CaculateSize(value.Key) + ValueSerializer.CaculateSize(value.Value) + offset;
        }

        public void Serialize(in KeyValuePair<TKey, TValue> value, Span<byte> storage)
        {
            if (IsFixedSize)
            {
                var keySize = KeySerializer.CaculateSize(default);
                var keyStorage = storage.Slice(0, keySize);
                var valueStorage = storage.Slice(keySize);
                KeySerializer.Serialize(value.Key, keyStorage);
                ValueSerializer.Serialize(value.Value, valueStorage);
            }
            else
            {
                var keySize = KeySerializer.CaculateSize(value.Key);
                storage.NonPortableCast<byte, int>()[0] = keySize;
                var keyStorage = storage.Slice(offset, keySize);
                var valueStorage = storage.Slice(offset + keySize);
                KeySerializer.Serialize(value.Key, keyStorage);
                ValueSerializer.Serialize(value.Value, valueStorage);
            }
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref KeyValuePair<TKey, TValue> value)
        {
            if (IsFixedSize)
            {
                var keySize = KeySerializer.CaculateSize(default);
                var keyStorage = storage.Slice(0, keySize);
                var valueStorage = storage.Slice(keySize);
                var k = default(TKey);
                var v = default(TValue);
                KeySerializer.Deserialize(keyStorage, ref k);
                ValueSerializer.Deserialize(valueStorage, ref v);
                value = new KeyValuePair<TKey, TValue>(k, v);
            }
            else
            {
                var keySize = storage.NonPortableCast<byte, int>()[0];
                var keyStorage = storage.Slice(offset, keySize);
                var valueStorage = storage.Slice(offset + keySize);
                var k = default(TKey);
                var v = default(TValue);
                KeySerializer.Deserialize(keyStorage, ref k);
                ValueSerializer.Deserialize(valueStorage, ref v);
                value = new KeyValuePair<TKey, TValue>(k, v);
            }
        }
    }
}
