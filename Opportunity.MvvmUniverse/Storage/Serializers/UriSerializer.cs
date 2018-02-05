using System;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class UriSerializer : ISerializer<Uri>
    {
        private const int offset = sizeof(char);

        public int CaculateSize(in Uri value)
        {
            if (value == null)
                return 0;
            var r = value.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
            return r.Length * sizeof(char) + offset;
        }

        public void Serialize(in Uri value, Span<byte> storage)
        {
            if (value == null)
                return;
            if (!value.IsAbsoluteUri)
                storage[0] = 0x01;
            var chars = storage.Slice(offset).NonPortableCast<byte, char>();
            value.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped).AsSpan().CopyTo(chars);
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref Uri value)
        {
            if (storage.IsEmpty)
            {
                value = null;
                return;
            }
            var isAbsoluteUri = storage.NonPortableCast<byte, char>()[0] == 0;
            var data = new string(storage.Slice(offset).NonPortableCast<byte, char>().ToArray());
            if (isAbsoluteUri)
                value = new Uri(data, UriKind.Absolute);
            else
                value = new Uri(data, UriKind.Relative);
        }
    }
}
