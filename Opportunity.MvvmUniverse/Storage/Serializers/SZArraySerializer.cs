using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Storage.Serializers
{
    public sealed class SZArraySerializer<TElement> : ISerializer<TElement[]>
    {
        private ISerializer<TElement> elementSerializer = Serializer<TElement>.Default;
        public ISerializer<TElement> ElementSerializer
        {
            get => this.elementSerializer;
            set => this.elementSerializer = value ?? Serializer<TElement>.Default;
        }

        private Type predictedStorageElementType;
        private Func<object, ISerializer<TElement>, TElement[]> predictedDeserializer;
        private Func<TElement[], ISerializer<TElement>, object> predictedSerializer;

        private bool fillDelegates(Type storageElementType)
        {
            try
            {
                var des = DeserializeCore.Dic[storageElementType];
                var ser = SerializeCore.Dic[storageElementType];
                this.predictedStorageElementType = storageElementType;
                this.predictedDeserializer = des;
                this.predictedSerializer = ser;
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        private static class DeserializeCore
        {
            static TElement[] Cbool(object sto, ISerializer<TElement> ser) => Core((bool[])sto, ser);

            static TElement[] Cbyte(object sto, ISerializer<TElement> ser) => Core((byte[])sto, ser);
            static TElement[] Cushort(object sto, ISerializer<TElement> ser) => Core((ushort[])sto, ser);
            static TElement[] Cuint(object sto, ISerializer<TElement> ser) => Core((uint[])sto, ser);
            static TElement[] Culong(object sto, ISerializer<TElement> ser) => Core((ulong[])sto, ser);

            static TElement[] Cfloat(object sto, ISerializer<TElement> ser) => Core((float[])sto, ser);
            static TElement[] Cdouble(object sto, ISerializer<TElement> ser) => Core((double[])sto, ser);

            static TElement[] Cshort(object sto, ISerializer<TElement> ser) => Core((short[])sto, ser);
            static TElement[] Cint(object sto, ISerializer<TElement> ser) => Core((int[])sto, ser);
            static TElement[] Clong(object sto, ISerializer<TElement> ser) => Core((long[])sto, ser);

            static TElement[] Cchar(object sto, ISerializer<TElement> ser) => Core((char[])sto, ser);
            static TElement[] Cstring(object sto, ISerializer<TElement> ser) => Core((string[])sto, ser);

            static TElement[] CGuid(object sto, ISerializer<TElement> ser) => Core((Guid[])sto, ser);

            static TElement[] CDateTimeOffset(object sto, ISerializer<TElement> ser) => Core((DateTimeOffset[])sto, ser);
            static TElement[] CTimeSpan(object sto, ISerializer<TElement> ser) => Core((TimeSpan[])sto, ser);

            static TElement[] CPoint(object sto, ISerializer<TElement> ser) => Core((Point[])sto, ser);
            static TElement[] CRect(object sto, ISerializer<TElement> ser) => Core((Rect[])sto, ser);
            static TElement[] CSize(object sto, ISerializer<TElement> ser) => Core((Size[])sto, ser);

            public static TElement[] Core<TStorage>(TStorage[] storage, ISerializer<TElement> serializer)
            {
                var r = new TElement[storage.Length];
                for (var i = 0; i < storage.Length; i++)
                {
                    r[i] = serializer.Deserialize(storage[i]);
                }
                return r;
            }

            public static readonly Dictionary<Type, Func<object, ISerializer<TElement>, TElement[]>> Dic
                = new Dictionary<Type, Func<object, ISerializer<TElement>, TElement[]>>
                {
                    [typeof(bool)] = Cbool,

                    [typeof(byte)] = Cbyte,
                    [typeof(ushort)] = Cushort,
                    [typeof(uint)] = Cuint,
                    [typeof(ulong)] = Culong,

                    [typeof(float)] = Cfloat,
                    [typeof(double)] = Cdouble,

                    [typeof(short)] = Cshort,
                    [typeof(int)] = Cint,
                    [typeof(long)] = Clong,

                    [typeof(char)] = Cchar,
                    [typeof(string)] = Cstring,

                    [typeof(Guid)] = CGuid,

                    [typeof(DateTimeOffset)] = CDateTimeOffset,
                    [typeof(TimeSpan)] = CTimeSpan,

                    [typeof(Point)] = CPoint,
                    [typeof(Rect)] = CRect,
                    [typeof(Size)] = CSize,
                };
        }

        public TElement[] Deserialize(object value)
        {
            if (value is string hint)
            {
                switch (hint)
                {
                case NULL_HINT:
                    return null;
                case ZERO_LENGTH_HINT:
                default:
                    return Array.Empty<TElement>();
                }
            }
            if (value is TElement[] array && this.elementSerializer is WinRTBaseDataTypeSerializer<TElement>)
                return array;
            var storageElementType = value.GetType().GetElementType();
            if (storageElementType == this.predictedStorageElementType)
                return this.predictedDeserializer(value, this.elementSerializer);
            else if (fillDelegates(storageElementType))
                return this.predictedDeserializer(value, this.elementSerializer);
            return default;
        }

        private static class SerializeCore
        {
            public static object Core(TElement[] value, ISerializer<TElement> serializer)
            {
                var type = default(Type);
                var storage = new object[value.Length];
                for (var i = 0; i < value.Length; i++)
                {
                    var s = serializer.Serialize(value[i]);
                    storage[i] = s ?? throw new NotSupportedException($"Element at {i}({value[i]}) serialized to null, which is not supported");

                    var sType = s.GetType();
                    if (type != null && type != sType)
                        throw new InvalidOperationException("Mixed element serialize types, all elements should be serialized to single WinRT type");
                    else
                        type = sType;
                }
                var typedStorage = Array.CreateInstance(type, storage.Length);
                Array.Copy(storage, typedStorage, storage.Length);
                return typedStorage;
            }

            private static TStorage[] Core<TStorage>(TElement[] value, ISerializer<TElement> serializer)
            {
                var storage = new TStorage[value.Length];
                for (var i = 0; i < value.Length; i++)
                {
                    try
                    {
                        storage[i] = (TStorage)serializer.Serialize(value[i]);
                    }
                    catch (InvalidCastException)
                    {
                        return null;
                    }
                }
                return storage;
            }

            public static readonly Dictionary<Type, Func<TElement[], ISerializer<TElement>, object>> Dic
                = new Dictionary<Type, Func<TElement[], ISerializer<TElement>, object>>
                {
                    [typeof(bool)] = Core<bool>,

                    [typeof(byte)] = Core<byte>,
                    [typeof(ushort)] = Core<ushort>,
                    [typeof(uint)] = Core<uint>,
                    [typeof(ulong)] = Core<ulong>,

                    [typeof(float)] = Core<float>,
                    [typeof(double)] = Core<double>,

                    [typeof(short)] = Core<short>,
                    [typeof(int)] = Core<int>,
                    [typeof(long)] = Core<long>,

                    [typeof(char)] = Core<char>,
                    [typeof(string)] = Core<string>,

                    [typeof(Guid)] = Core<Guid>,

                    [typeof(DateTimeOffset)] = Core<DateTimeOffset>,
                    [typeof(TimeSpan)] = Core<TimeSpan>,

                    [typeof(Point)] = Core<Point>,
                    [typeof(Rect)] = Core<Rect>,
                    [typeof(Size)] = Core<Size>,
                };
        }

        public object Serialize(TElement[] value)
        {
            if (value == null)
                return NULL_HINT;
            if (value.Length == 0)
                return ZERO_LENGTH_HINT;
            if (this.elementSerializer is WinRTBaseDataTypeSerializer<TElement>)
                return value;
            var dele = this.predictedSerializer;
            if (dele != null)
            {
                var r = dele(value, this.elementSerializer);
                if (r != null)
                    return r;
            }
            var result = SerializeCore.Core(value, this.elementSerializer);
            if (result is string)
                return result;
            fillDelegates(result.GetType().GetElementType());
            return result;
        }

        private const string ZERO_LENGTH_HINT = "Zero length SZArray";
        private const string NULL_HINT = "Null";
    }
}
