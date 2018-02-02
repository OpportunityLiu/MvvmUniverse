using System;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class UriSerializer : ISerializer<Uri>
    {

        public Uri Deserialize(object value)
        {
            if (!(value is string uriString))
                return null;
            if (uriString == NULL_HINT)
                return null;
            if (Uri.TryCreate(uriString, UriKind.Absolute, out var r))
                return r;
            if (Uri.TryCreate(uriString, UriKind.Relative, out r))
                return r;
            throw new InvalidOperationException("Can't deserialize this string")
            {
                Data =
                {
                    ["Uri"] = uriString,
                },
            };
        }

        public object Serialize(Uri value)
        {
            if (value == null)
                return NULL_HINT;
            var r = value.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
            if (r == NULL_HINT)
                return "";
            return r;
        }

        // Do not change!!
        private const string NULL_HINT = " ";
    }
}
