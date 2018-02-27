using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

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

        public void Serialize(in T value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteUInt32(uint.MaxValue);
                return;
            }
            using (var ms = new MemoryStream())
            {
                this.xmlSerializer.Serialize(ms, value);
                if (!ms.TryGetBuffer(out var buf))
                    throw new InvalidOperationException("Can't get buffer of memory stream.");
                storage.WriteUInt32((uint)buf.Count);
                storage.WriteBuffer(buf.Array.AsBuffer(), (uint)buf.Offset, (uint)buf.Count);
            }
        }

        public void Deserialize(DataReader storage, ref T value)
        {
            var length = storage.ReadUInt32();
            if (length == uint.MaxValue)
            {
                value = default;
                return;
            }
            var data = storage.ReadBuffer(length);
            using (var ms = data.AsStream())
            {
                value = (T)this.xmlSerializer.Deserialize(ms);
            }
        }
    }
}
