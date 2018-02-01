using System;

namespace Opportunity.MvvmUniverse.Settings.Serializers
{
    public sealed class UriSerializer : ISerializer<Uri>
    {
        public Uri Deserialize(object value)
        {
            if (value == null)
                return null;
            if (Uri.TryCreate(value.ToString(), UriKind.Absolute, out var r))
                return r;
            if (Uri.TryCreate(value.ToString(), UriKind.Relative, out r))
                return r;
            throw new InvalidOperationException("Can't deserialize this string")
            {
                Data =
                {
                    ["Uri"] = value.ToString(),
                },
            };
        }
        public object Serialize(Uri value)
        {
            if (value == null)
                return null;
            return value.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
        }
    }
}
