namespace Opportunity.MvvmUniverse.Settings.Serializers
{
    public sealed class EmptySerializer<T> : ISerializer<T>
    {
        public T Deserialize(object value) => (T)value;
        public object Serialize(T value) => value;
    }
}
