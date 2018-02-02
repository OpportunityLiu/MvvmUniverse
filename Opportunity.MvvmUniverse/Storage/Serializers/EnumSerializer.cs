using System;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class EnumSerializer<T> : ISerializer<T>
    {
        public T Deserialize(object value)
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        public object Serialize(T value)
        {
            return ((Enum)(object)value).ToUInt64();
        }
    }

    public sealed class EnumStringSerializer<T> : ISerializer<T>
        where T : struct
    {
        public T Deserialize(object value)
        {
            if (Enum.TryParse<T>(value.ToString(), out var r))
                return r;
            return default;
        }

        public object Serialize(T value)
        {
            return value.ToString();
        }
    }
}
