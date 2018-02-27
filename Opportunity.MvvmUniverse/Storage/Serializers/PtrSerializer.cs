using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class PtrSerializer :
        ISerializer<UIntPtr>, ISerializer<IntPtr>
    {
        public static PtrSerializer Instance { get; } = new PtrSerializer();

        void ISerializer<UIntPtr>.Serialize(in UIntPtr value, DataWriter storage) => storage.WriteUInt64((ulong)value);
        void ISerializer<UIntPtr>.Deserialize(DataReader storage, ref UIntPtr value) => value = new UIntPtr(storage.ReadUInt64());

        void ISerializer<IntPtr>.Serialize(in IntPtr value, DataWriter storage) => storage.WriteInt64((long)value);
        void ISerializer<IntPtr>.Deserialize(DataReader storage, ref IntPtr value) => value = new IntPtr(storage.ReadInt64());
    }
}
