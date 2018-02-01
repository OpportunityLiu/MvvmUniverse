namespace Opportunity.MvvmUniverse.Settings.Serializers
{
    public sealed class SByteSerializer : ISerializer<sbyte>
    {
        public sbyte Deserialize(object value) => unchecked((sbyte)(short)value);
        public object Serialize(sbyte value) => unchecked((short)value);
    }
}
