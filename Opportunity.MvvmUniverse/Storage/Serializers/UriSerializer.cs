using System;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class UriSerializer : ISerializer<Uri>
    {
        public void Serialize(in Uri value, DataWriter storage)
        {
            var str = value?.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
            Serializer<string>.Default.Serialize(in str, storage);
        }

        public void Deserialize(DataReader storage, ref Uri value)
        {
            var str = default(string);
            Serializer<string>.Default.Deserialize(storage, ref str);
            if (str == null)
            {
                value = null;
                return;
            }
            value = new Uri(str, UriKind.RelativeOrAbsolute);
        }
    }
}
