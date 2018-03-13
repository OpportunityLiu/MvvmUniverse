using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    /// <summary>
    /// <see cref="ISerializer{T}"/> based on <see cref="System.Xml.Serialization.XmlSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements.</typeparam>
    public sealed class XmlSerializer<T> : ISerializer<T>
    {
        /// <summary>
        /// Create new instance of <see cref="XmlSerializer{T}"/>.
        /// </summary>
        public XmlSerializer()
            : this(new System.Xml.Serialization.XmlSerializer(typeof(T))) { }

        /// <summary>
        /// Create new instance of <see cref="XmlSerializer{T}"/>.
        /// </summary>
        /// <param name="xmlSerializer"><see cref="System.Xml.Serialization.XmlSerializer"/> used for this instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="xmlSerializer"/> is <see langword="null"/>.</exception>
        public XmlSerializer(System.Xml.Serialization.XmlSerializer xmlSerializer)
        {
            this.xmlSerializer = xmlSerializer ?? throw new ArgumentNullException(nameof(xmlSerializer));
        }

        private System.Xml.Serialization.XmlSerializer xmlSerializer;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
