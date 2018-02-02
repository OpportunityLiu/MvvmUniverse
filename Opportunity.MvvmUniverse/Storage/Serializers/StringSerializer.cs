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
        public string Deserialize(object value)
        {
            if (!(value is string s))
                return null;
            if (s == "null")
                return null;
            if (s.StartsWith("null"))
            {
                for (var i = 4; i < s.Length; i++)
                {
                    if (s[i] != '_')
                        return s;
                }
                return s.Substring(0, s.Length - 1);
            }
            return s;
        }

        public object Serialize(string value)
        {
            if (value == null)
                return "null";
            if (value.StartsWith("null"))
            {
                for (var i = 4; i < value.Length; i++)
                {
                    if (value[i] != '_')
                        return value;
                }
                return value + "_";
            }
            return value;
        }
    }
}
