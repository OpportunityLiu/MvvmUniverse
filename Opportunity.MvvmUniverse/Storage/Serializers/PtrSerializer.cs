using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class PtrSerializer : ISerializer<IntPtr>, ISerializer<UIntPtr>
    {
        public object Deserialize(object value)
        {
            switch (value)
            {
            case ulong u: return new UIntPtr(u);
            case long i: return new IntPtr(i);
            default: return null;
            }
        }

        public object Serialize(IntPtr value) => value.ToInt64();
        public object Serialize(UIntPtr value) => value.ToUInt64();
        IntPtr ISerializer<IntPtr>.Deserialize(object value) => new IntPtr((value as long?) ?? 0L);
        UIntPtr ISerializer<UIntPtr>.Deserialize(object value) => new UIntPtr((value as ulong?) ?? 0UL);
    }
}
