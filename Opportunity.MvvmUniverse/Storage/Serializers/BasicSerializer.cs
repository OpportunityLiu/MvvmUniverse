using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    internal sealed class BasicSerializer :
        ISerializer<bool>, ISerializer<Guid>, ISerializer<char>,
        ISerializer<byte>, ISerializer<byte[]>, ISerializer<sbyte>,
        ISerializer<ushort>, ISerializer<short>,
        ISerializer<uint>, ISerializer<int>,
        ISerializer<ulong>, ISerializer<long>,
        ISerializer<float>, ISerializer<double>,
        ISerializer<DateTime>, ISerializer<DateTimeOffset>, ISerializer<TimeSpan>
    {
        public static BasicSerializer Instance { get; } = new BasicSerializer();

        void ISerializer<bool>.Serialize(in bool value, DataWriter storage) => storage.WriteBoolean(value);
        void ISerializer<bool>.Deserialize(DataReader storage, ref bool value) => value = storage.ReadBoolean();

        void ISerializer<Guid>.Serialize(in Guid value, DataWriter storage) => storage.WriteGuid(value);
        void ISerializer<Guid>.Deserialize(DataReader storage, ref Guid value) => value = storage.ReadGuid();

        void ISerializer<char>.Serialize(in char value, DataWriter storage) => storage.WriteUInt16(value);
        void ISerializer<char>.Deserialize(DataReader storage, ref char value) => value = (char)storage.ReadUInt16();

        void ISerializer<byte>.Serialize(in byte value, DataWriter storage) => storage.WriteByte(value);
        void ISerializer<byte>.Deserialize(DataReader storage, ref byte value) => value = storage.ReadByte();

        void ISerializer<byte[]>.Serialize(in byte[] value, DataWriter storage)
        {
            if (value == null)
            {
                storage.WriteInt32(-1);
                return;
            }
            storage.WriteInt32(value.Length);
            if (value.Length > 0)
                storage.WriteBytes(value);
        }
        void ISerializer<byte[]>.Deserialize(DataReader storage, ref byte[] value)
        {
            var length = storage.ReadInt32();
            if (length < 0)
                value = null;
            else if (length == 0)
                value = Array.Empty<byte>();
            else
            {
                if (value?.Length != length)
                    value = new byte[length];
                storage.ReadBytes(value);
            }
        }

        void ISerializer<sbyte>.Serialize(in sbyte value, DataWriter storage) => storage.WriteByte(unchecked((byte)value));
        void ISerializer<sbyte>.Deserialize(DataReader storage, ref sbyte value) => value = unchecked((sbyte)storage.ReadByte());

        void ISerializer<ushort>.Serialize(in ushort value, DataWriter storage) => storage.WriteUInt16(value);
        void ISerializer<ushort>.Deserialize(DataReader storage, ref ushort value) => value = storage.ReadUInt16();

        void ISerializer<short>.Serialize(in short value, DataWriter storage) => storage.WriteInt16(value);
        void ISerializer<short>.Deserialize(DataReader storage, ref short value) => value = storage.ReadInt16();

        void ISerializer<uint>.Serialize(in uint value, DataWriter storage) => storage.WriteUInt32(value);
        void ISerializer<uint>.Deserialize(DataReader storage, ref uint value) => value = storage.ReadUInt32();

        void ISerializer<int>.Serialize(in int value, DataWriter storage) => storage.WriteInt32(value);
        void ISerializer<int>.Deserialize(DataReader storage, ref int value) => value = storage.ReadInt32();

        void ISerializer<ulong>.Serialize(in ulong value, DataWriter storage) => storage.WriteUInt64(value);
        void ISerializer<ulong>.Deserialize(DataReader storage, ref ulong value) => value = storage.ReadUInt64();

        void ISerializer<long>.Serialize(in long value, DataWriter storage) => storage.WriteInt64(value);
        void ISerializer<long>.Deserialize(DataReader storage, ref long value) => value = storage.ReadInt64();

        void ISerializer<float>.Serialize(in float value, DataWriter storage) => storage.WriteSingle(value);
        void ISerializer<float>.Deserialize(DataReader storage, ref float value) => value = storage.ReadSingle();

        void ISerializer<double>.Serialize(in double value, DataWriter storage) => storage.WriteDouble(value);
        void ISerializer<double>.Deserialize(DataReader storage, ref double value) => value = storage.ReadDouble();

        void ISerializer<DateTime>.Serialize(in DateTime value, DataWriter storage) => storage.WriteInt64(value.ToBinary());
        void ISerializer<DateTime>.Deserialize(DataReader storage, ref DateTime value) => value = DateTime.FromBinary(storage.ReadInt64());

        void ISerializer<DateTimeOffset>.Serialize(in DateTimeOffset value, DataWriter storage) => storage.WriteDateTime(value);
        void ISerializer<DateTimeOffset>.Deserialize(DataReader storage, ref DateTimeOffset value) => value = storage.ReadDateTime();

        void ISerializer<TimeSpan>.Serialize(in TimeSpan value, DataWriter storage) => storage.WriteTimeSpan(value);
        void ISerializer<TimeSpan>.Deserialize(DataReader storage, ref TimeSpan value) => value = storage.ReadTimeSpan();
    }
}
