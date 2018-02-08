using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class StringSerializer : ISerializer<string>
    {
        public StringSerializer() { }
        public StringSerializer(Encoding encoding)
        {
            this.encoding = encoding;
        }

        private const int offset = sizeof(int);

        private readonly Encoding encoding;
        public Encoding Encoding => this.encoding;

        public int CaculateSize(in string value)
        {
            if (value == null)
                return 0;
            if (this.encoding == null)
                return value.Length * sizeof(char) + offset;
            return this.encoding.GetByteCount(value) + offset;
        }

        public bool IsFixedSize => false;

        public void Serialize(in string value, Span<byte> storage)
        {
            if (value == null)
                return;
            if (this.encoding == null)
            {
                storage.Slice(0, offset).Clear();
                var chars = storage.Slice(offset).NonPortableCast<byte, char>();
                value.AsSpan().CopyTo(chars);
                return;
            }
            storage.NonPortableCast<byte, int>()[0] = this.encoding.CodePage;
            var bytes = this.encoding.GetBytes(value);
            bytes.AsSpan().CopyTo(storage.Slice(offset));
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref string value)
        {
            if (storage.IsEmpty)
            {
                value = null;
                return;
            }
            var codePage = storage.NonPortableCast<byte, int>()[0];
            var str = storage.Slice(offset);
            if (codePage == 0)
            {
                value = new string(str.NonPortableCast<byte, char>().ToArray());
                return;
            }
            var encoding = Encoding.GetEncoding(codePage);
            value = encoding.GetString(str.ToArray());
        }
    }
}
