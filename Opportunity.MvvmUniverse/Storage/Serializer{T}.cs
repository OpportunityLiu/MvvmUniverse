using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Storage.Serializers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Storage
{
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serialize value to storage value.
        /// </summary>
        /// <param name="value">value to serialize</param>
        /// <returns>
        /// An object of <see cref="Serializer.WinRTBaseDataTypes"/>,
        /// and should not be <c>null</c>, even if <paramref name="value"/> is <c>null</c>,
        /// only single type should be used.
        /// </returns>
        object Serialize(T value);

        /// <summary>
        /// Deserialize storage value to value.
        /// </summary>
        /// <param name="value">storage value to serialize, will not be <c>null</c></param>
        /// <returns>deserialized value, <c>null</c> or <c>default(T)</c> is valid</returns>
        T Deserialize(object value);
    }

    public abstract class Serializer
    {
        //bool

        //byte
        //ushort
        //uint
        //ulong

        //float
        //double

        //short
        //int
        //long

        //char
        //string

        //Guid

        //DateTimeOffset
        //TimeSpan

        //Point
        //Rect
        //Size
        internal static readonly Dictionary<Type, string> WinRTTypes = new Dictionary<Type, string>
        {
            [typeof(bool)] = "Boolean",

            [typeof(byte)] = "UInt8",
            [typeof(ushort)] = "UInt16",
            [typeof(uint)] = "UInt32",
            [typeof(ulong)] = "UInt64",

            [typeof(float)] = "float",
            [typeof(double)] = "double",

            [typeof(short)] = "Int16",
            [typeof(int)] = "Int32",
            [typeof(long)] = "Int64",

            [typeof(char)] = "Char16",
            [typeof(string)] = "String",

            [typeof(Guid)] = "Guid",

            [typeof(DateTimeOffset)] = "DateTime",
            [typeof(TimeSpan)] = "TimeSpan",

            [typeof(Point)] = "Point",
            [typeof(Rect)] = "Rect",
            [typeof(Size)] = "Size",
        };
        internal static readonly Dictionary<string, Type> WinRTTypesInv = inv(WinRTTypes);

        private static Dictionary<TV, TK> inv<TK, TV>(Dictionary<TK, TV> old)
        {
            var r = new Dictionary<TV, TK>();
            foreach (var item in old)
            {
                r.Add(item.Value, item.Key);
            }
            return r;
        }

        public static IReadOnlyCollection<Type> WinRTBaseDataTypes { get; } = WinRTTypes.Keys;
    }

    public abstract class Serializer<T> : Serializer, ISerializer<T>
    {
        public static ISerializer<T> Default { get; } = createDefault();
        private static ISerializer<T> createDefault()
        {
            try
            {
                var info = TypeTraits.Of<T>();

                if (typeof(T) == typeof(string))
                    return (ISerializer<T>)(object)new StringSerializer();
                if (WinRTTypes.ContainsKey(typeof(T)))
                    return new WinRTBaseDataTypeSerializer<T>();
                if (info.Type.IsEnum)
                    return new EnumSerializer<T>();
                if (typeof(T) == typeof(Uri))
                    return (ISerializer<T>)(object)new UriSerializer();
                if (typeof(T) == typeof(DateTime))
                    return (ISerializer<T>)(object)new DateTimeSerializer();
                if (typeof(T) == typeof(sbyte))
                    return (ISerializer<T>)(object)new SByteSerializer();
                if (typeof(T) == typeof(Color))
                    return (ISerializer<T>)(object)new ColorSerializer();
                if (typeof(T) == typeof(SolidColorBrush))
                    return (ISerializer<T>)(object)new SolidColorBrushSerializer();
                if (typeof(T) == typeof(UIntPtr) || typeof(T) == typeof(IntPtr))
                    return (ISerializer<T>)(object)new PtrSerializer();
                if (typeof(T) == typeof(UIntPtr) || typeof(T) == typeof(IntPtr))
                    return (ISerializer<T>)(object)new PtrSerializer();
                if (info.Type.IsArray && info.Type.GetArrayRank() == 1 && !info.Type.Name.Contains("[*]"))
                {
                    var ele = info.Type.AsType().GetElementType();
                    return (ISerializer<T>)Activator.CreateInstance(typeof(SZArraySerializer<>).MakeGenericType(ele));
                }
                {
                    var q = from type in Enumerable.Repeat(info.Type.AsType(), 1).Concat(info.Type.ImplementedInterfaces)
                            where type.GenericTypeArguments.Length == 1 && type == typeof(ICollection<>).MakeGenericType(type.GenericTypeArguments)
                            select type.GenericTypeArguments[0];
                    var ele = q.FirstOrDefault();
                    if (ele != null)
                        return (ISerializer<T>)Activator.CreateInstance(typeof(CollectionSerializer<,>).MakeGenericType(typeof(T), ele));
                }
                throw new InvalidOperationException($"Default Serializer of unsupported type {typeof(T)} creating.");
            }
            catch (Exception ex)
            {
                // Unsupported
                Debug.WriteLine(ex);
                return new WinRTBaseDataTypeSerializer<T>();
            }
        }

        public abstract T Deserialize(object value);
        public abstract object Serialize(T value);

        public static Serializer<T> Create(Func<T, object> serializer, Func<object, T> deserializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));
            return new DelegateSerializer(serializer, deserializer);
        }

        private sealed class DelegateSerializer : Serializer<T>
        {
            private readonly Func<T, object> serializer;
            private readonly Func<object, T> deserializer;

            public DelegateSerializer(Func<T, object> serializer, Func<object, T> deserializer)
            {
                this.serializer = serializer;
                this.deserializer = deserializer;
            }

            public override T Deserialize(object value) => this.deserializer(value);
            public override object Serialize(T value) => this.serializer(value);
        }
    }
}
