using System;

namespace Opportunity.MvvmUniverse.Settings.Serializers
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
}
