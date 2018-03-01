using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Storage.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Windows.UI;

namespace Opportunity.MvvmUniverse.Storage
{
    /// <summary>
    /// Serizlizer for specific type.
    /// </summary>
    /// <typeparam name="T">Supported type.</typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serialize value to storage value.
        /// </summary>
        /// <param name="value">value to serialize</param>
        /// <param name="storage">storage to write into</param>
        void Serialize(in T value, DataWriter storage);
        /// <summary>
        /// Deserialize storage value to value.
        /// </summary>
        /// <param name="storage">storage to read from</param>
        /// <param name="value">value to deserialize, old value can be reused or replaced</param>
        void Deserialize(DataReader storage, ref T value);
    }

    /// <summary>
    /// Contains factory methods for <see cref="Serializer{T}"/> creation.
    /// </summary>
    public abstract class Serializer
    {
        /// <summary>
        /// Create a <see cref="Serializer{T}"/> implemented by delegates.
        /// </summary>
        /// <typeparam name="T">Supported type of <see cref="Serializer{T}"/>.</typeparam>
        /// <param name="serializer">Delegete for serialize</param>
        /// <param name="deserializer">Delegete for deserialize.</param>
        /// <returns>A <see cref="Serializer{T}"/> implmented by delegates.</returns>
        /// <exception cref="ArgumentNullException">Parameter is <see langword="null"/>.</exception>
        public static Serializer<T> Create<T>(Action<T, DataWriter> serializer, Func<DataReader, T, T> deserializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));
            return new DelegateSerializer<T>(serializer, deserializer);
        }

        private sealed class DelegateSerializer<T> : Serializer<T>
        {
            private readonly Action<T, DataWriter> serializer;
            private readonly Func<DataReader, T, T> deserializer;

            public DelegateSerializer(Action<T, DataWriter> serializer, Func<DataReader, T, T> deserializer)
            {
                this.serializer = serializer;
                this.deserializer = deserializer;
            }

            public override void Deserialize(DataReader storage, ref T value) => value = this.deserializer(storage, value);
            public override void Serialize(in T value, DataWriter storage) => this.serializer(value, storage);
        }

        internal static object SyncRoot { get; } = new object();

        static Serializer()
        {
            Serializer<string>.Storage.Default = new StringSerializer();
            Serializer<Uri>.Storage.Default = new UriSerializer();

            Serializer<bool>.Storage.Default = BasicSerializer.Instance;
            Serializer<Guid>.Storage.Default = BasicSerializer.Instance;
            Serializer<char>.Storage.Default = BasicSerializer.Instance;
            Serializer<byte>.Storage.Default = BasicSerializer.Instance;
            Serializer<byte[]>.Storage.Default = BasicSerializer.Instance;
            Serializer<sbyte>.Storage.Default = BasicSerializer.Instance;
            Serializer<ushort>.Storage.Default = BasicSerializer.Instance;
            Serializer<short>.Storage.Default = BasicSerializer.Instance;
            Serializer<uint>.Storage.Default = BasicSerializer.Instance;
            Serializer<int>.Storage.Default = BasicSerializer.Instance;
            Serializer<ulong>.Storage.Default = BasicSerializer.Instance;
            Serializer<long>.Storage.Default = BasicSerializer.Instance;
            Serializer<float>.Storage.Default = BasicSerializer.Instance;
            Serializer<double>.Storage.Default = BasicSerializer.Instance;
            Serializer<DateTime>.Storage.Default = BasicSerializer.Instance;
            Serializer<DateTimeOffset>.Storage.Default = BasicSerializer.Instance;
            Serializer<TimeSpan>.Storage.Default = BasicSerializer.Instance;

            Serializer<UIntPtr>.Storage.Default = PtrSerializer.Instance;
            Serializer<IntPtr>.Storage.Default = PtrSerializer.Instance;

            Serializer<Color>.Storage.Default = ColorSerializer.Instance;
            Serializer<SolidColorBrush>.Storage.Default = ColorSerializer.Instance;
        }
    }

    /// <summary>
    /// Base class for serializers implements <see cref="ISerializer{T}"/>.
    /// </summary>
    /// <typeparam name="T">Supported type.</typeparam>
    public abstract class Serializer<T> : Serializer, ISerializer<T>
    {
        internal static class Storage
        {
            public static ISerializer<T> Default;
            public static ISerializer<T> Value;
        }

        private static ISerializer<T> createDefault()
        {
            var info = TypeTraits.Of<T>();
            var tType = info.Type;

            if (tType.IsEnum)
                return (ISerializer<T>)Activator.CreateInstance(typeof(EnumSerializer<>).MakeGenericType(tType.AsType()));

            var tGenericDef = tType.IsGenericType ? tType.GetGenericTypeDefinition() : null;
            if (tGenericDef == typeof(KeyValuePair<,>))
            {
                return (ISerializer<T>)Activator.CreateInstance(typeof(KeyValuePairSerializer<,>).MakeGenericType(tType.GenericTypeArguments));
            }

            if (tType.IsArray && tType.GetArrayRank() == 1 && !tType.Name.Contains("[*]"))
            {
                var ele = tType.GetElementType();
                return (ISerializer<T>)Activator.CreateInstance(typeof(SZArraySerializer<>).MakeGenericType(ele));
            }
            var tIsInterface = tType.IsInterface;
            var tInterfaces = tType.ImplementedInterfaces.ToArray();

            var tGenericDefInterface = new Type[tInterfaces.Length];
            for (var i = 0; i < tGenericDefInterface.Length; i++)
            {
                var inter = tInterfaces[i];
                tGenericDefInterface[i] = inter.IsConstructedGenericType ? inter.GetGenericTypeDefinition() : null;
            }

            var r = tryGet1(typeof(IList<>), tType.AsType(), tGenericDef, tInterfaces, tGenericDefInterface, typeof(ListSerializer<,>));
            if (r != null)
                return r;

            r = tryGet1(typeof(ICollection<>), tType.AsType(), tGenericDef, tInterfaces, tGenericDefInterface, typeof(CollectionSerializer<,>));
            if (r != null)
                return r;

            //if (default(T) != null /* && !RuntimeHelpers.IsReferenceOrContainsReferences<T>()*/)
            //    return (ISerializer<T>)Activator.CreateInstance(typeof(StructSerializer<>).MakeGenericType(typeof(T)));
            if (info.IsNullable)
                return (ISerializer<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(info.NullableUnderlyingType.AsType()));

            return new XmlSerializer<T>();
        }

        private static ISerializer<T> tryGet1(Type gType, Type tType, Type tGenericDef, Type[] tInterfaces, Type[] tGenericDefInterface, Type sType)
        {
            var eleType = default(Type);
            if (tGenericDef == gType)
                eleType = tType.GenericTypeArguments[0];
            else
            {
                for (var i = 0; i < tGenericDefInterface.Length; i++)
                {
                    if (tGenericDefInterface[i] == gType)
                    {
                        eleType = tInterfaces[i].GenericTypeArguments[0];
                        break;
                    }
                }
            }
            if (eleType != null)
                return (ISerializer<T>)Activator.CreateInstance(sType.MakeGenericType(tType, eleType));
            return null;
        }

        /// <summary>
        /// Default <see cref="ISerializer{T}"/> for current type.
        /// Value could be <see langword="null"/>.
        /// </summary>
        public static ISerializer<T> Default
        {
            get
            {
                var value = Storage.Value;
                if (value != null)
                    return value;
                if (Storage.Default == null)
                    lock (SyncRoot)
                        if (Storage.Default == null)
                            Storage.Default = createDefault();
                return Storage.Default;
            }
            set => Storage.Value = value;
        }

        /// <summary>
        /// Serialize value to storage value.
        /// </summary>
        /// <param name="value">value to serialize</param>
        /// <param name="storage">storage to write into</param>
        public abstract void Serialize(in T value, DataWriter storage);
        /// <summary>
        /// Deserialize storage value to value.
        /// </summary>
        /// <param name="storage">storage to read from</param>
        /// <param name="value">value to deserialize, old value can be reused or replaced</param>
        public abstract void Deserialize(DataReader storage, ref T value);
    }
}
