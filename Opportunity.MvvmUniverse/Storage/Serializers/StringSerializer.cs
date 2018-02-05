using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// Use this serializer to avoid null string.
    /// Serialize <c>null</c> to <c>"null"</c> and <c>"null"</c> to <c>"null_"</c> and so on.
    /// </summary>
    public sealed class StringSerializer : ISerializer<string>
    {
        private const int offset = sizeof(char);

        public int CaculateSize(in string value)
        {
            if (value == null)
                return 0;
            return value.Length * sizeof(char) + offset;
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref string value)
        {
            if (storage.IsEmpty)
            {
                value = null;
                return;
            }
            value = new string(storage.Slice(offset).NonPortableCast<byte, char>().ToArray());
        }

        public void Serialize(in string value, Span<byte> storage)
        {
            if (value == null)
                return;
            storage.Slice(0, offset).Clear();
            var chars = storage.Slice(offset).NonPortableCast<byte, char>();
            value.AsSpan().CopyTo(chars);
        }
    }
}
