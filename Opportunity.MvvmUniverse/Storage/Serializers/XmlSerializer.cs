using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class XmlSerializer<T> : ISerializer<T>
    {
        public XmlSerializer()
            : this(new System.Xml.Serialization.XmlSerializer(typeof(T))) { }

        public XmlSerializer(System.Xml.Serialization.XmlSerializer xmlSerializer)
        {
            this.xmlSerializer = xmlSerializer ?? throw new ArgumentNullException(nameof(xmlSerializer));
        }

        private System.Xml.Serialization.XmlSerializer xmlSerializer;
        private static readonly int offset = sizeof(int);

        public int CaculateSize(in T value)
        {
            if (value == null)
                return 0;
            using (var ms = new MemoryStream())
            {
                this.xmlSerializer.Serialize(ms, value);
                return (int)ms.Length + offset;
            }
        }

        public bool IsFixedSize => false;

        public void Serialize(in T value, Span<byte> storage)
        {
            if (value == null)
                return;
            storage.NonPortableCast<byte, int>()[0] = Encoding.UTF8.CodePage;
            using (var ms = new MemoryStream(storage.Length))
            {
                this.xmlSerializer.Serialize(ms, value);
                if (!ms.TryGetBuffer(out var buf))
                    throw new InvalidOperationException("Can't get buffer of memory stream.");
                buf.AsSpan().Slice(0, storage.Length - offset).CopyTo(storage.Slice(offset));
            }
        }

        public void Deserialize(ReadOnlySpan<byte> storage, ref T value)
        {
            if (storage.IsEmpty)
            {
                value = default;
                return;
            }
            using (var ms = new MemoryStream(storage.Slice(offset).ToArray(), false))
            {
                value = (T)this.xmlSerializer.Deserialize(ms);
            }
        }
    }
}
