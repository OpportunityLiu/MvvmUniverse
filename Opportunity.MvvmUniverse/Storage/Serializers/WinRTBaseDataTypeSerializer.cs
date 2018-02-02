namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class WinRTBaseDataTypeSerializer<T> : ISerializer<T>
    {
        public T Deserialize(object value) => (T)value;
        public object Serialize(T value) => value;
    }
}
