using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class StringSerializer : ISerializer<string>
    {
        public void Serialize(in string value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteUInt32(uint.MaxValue);
                return;
            }
            if (value.Length == 0)
                storage.WriteUInt32(0);
            else
            {
                var l = storage.MeasureString(value);
                storage.WriteUInt32(l);
                storage.WriteString(value);
            }
        }

        public void Deserialize(DataReader storage, ref string value)
        {
            var length = storage.ReadUInt32();
            if (length == uint.MaxValue)
            {
                value = null;
                return;
            }
            if (length == 0)
            {
                value = "";
                return;
            }
            value = storage.ReadString(length);
        }
    }
}
