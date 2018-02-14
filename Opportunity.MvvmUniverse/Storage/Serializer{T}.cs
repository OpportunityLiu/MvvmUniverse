using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Storage.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Storage
{
    public interface ISerializer<T>
    {
        /// <summary>
        /// Returns <see langword="true"/> if <see cref="CaculateSize(T)"/> will return
        /// a same value regardless of the arguement, especially, <c>default(T)</c>,
        /// otherwise, <see langword="false"/>.
        /// </summary>
        bool IsFixedSize { get; }
        /// <summary>
        /// Caculate needed storage of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">object to serialize</param>
        /// <returns>storage size needed, in bytes</returns>
        int CaculateSize(in T value);
        /// <summary>
        /// Serialize value to storage value.
        /// </summary>
        /// <param name="value">value to serialize</param>
        /// <param name="storage">storage to write into</param>
        void Serialize(in T value, Span<byte> storage);
        /// <summary>
        /// Deserialize storage value to value.
        /// </summary>
        /// <param name="storage">storage to read from</param>
        /// <param name="value">value to deserialize, old value can be reused or replaced</param>
        void Deserialize(ReadOnlySpan<byte> storage, ref T value);
    }

    public abstract class Serializer
    {
        public static Serializer<T> Create<T>(Func<T, int> sizeCaculator, Action<T, Span<byte>> serializer, Func<ReadOnlySpan<byte>, T, T> deserializer)
        {
            if (sizeCaculator == null)
                throw new ArgumentNullException(nameof(sizeCaculator));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));
            return new DelegateSerializer<T>(sizeCaculator, serializer, deserializer);
        }

        public static Serializer<T> Create<T>(int size, Action<T, Span<byte>> serializer, Func<ReadOnlySpan<byte>, T, T> deserializer)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));
            return new FixedDelegateSerializer<T>(size, serializer, deserializer);
        }

        private sealed class DelegateSerializer<T> : Serializer<T>
        {
            private readonly Func<T, int> caculator;
            private readonly Action<T, Span<byte>> serializer;
            private readonly Func<ReadOnlySpan<byte>, T, T> deserializer;

            public DelegateSerializer(Func<T, int> caculator, Action<T, Span<byte>> serializer, Func<ReadOnlySpan<byte>, T, T> deserializer)
            {
                this.caculator = caculator;
                this.serializer = serializer;
                this.deserializer = deserializer;
            }

            public override bool IsFixedSize => false;

            public override int CaculateSize(in T value) => this.caculator(value);
            public override void Deserialize(ReadOnlySpan<byte> storage, ref T value) => value = this.deserializer(storage, value);
            public override void Serialize(in T value, Span<byte> storage) => this.serializer(value, storage);
        }

        private sealed class FixedDelegateSerializer<T> : Serializer<T>
        {
            private readonly int size;
            private readonly Action<T, Span<byte>> serializer;
            private readonly Func<ReadOnlySpan<byte>, T, T> deserializer;

            public FixedDelegateSerializer(int size, Action<T, Span<byte>> serializer, Func<ReadOnlySpan<byte>, T, T> deserializer)
            {
                this.size = size;
                this.serializer = serializer;
                this.deserializer = deserializer;
            }

            public override bool IsFixedSize => true;

            public override int CaculateSize(in T value) => this.size;
            public override void Deserialize(ReadOnlySpan<byte> storage, ref T value) => value = this.deserializer(storage, value);
            public override void Serialize(in T value, Span<byte> storage) => this.serializer(value, storage);
        }

        internal static object SyncRoot { get; } = new object();

        static Serializer()
        {
            Serializer<string>.Storage.Default = new StringSerializer();
            Serializer<Uri>.Storage.Default = new UriSerializer();
            Serializer<SolidColorBrush>.Storage.Default = new SolidColorBrushSerializer();

            Serializer<byte>.Storage.Default = new StructSerializer<byte>();
            Serializer<byte[]>.Storage.Default = new SZArraySerializer<byte>();
            Serializer<sbyte>.Storage.Default = new StructSerializer<sbyte>();
            Serializer<ushort>.Storage.Default = new StructSerializer<ushort>();
            Serializer<short>.Storage.Default = new StructSerializer<short>();
            Serializer<uint>.Storage.Default = new StructSerializer<uint>();
            Serializer<int>.Storage.Default = new StructSerializer<int>();
            Serializer<ulong>.Storage.Default = new StructSerializer<ulong>();
            Serializer<long>.Storage.Default = new StructSerializer<long>();
            Serializer<float>.Storage.Default = new StructSerializer<float>();
            Serializer<double>.Storage.Default = new StructSerializer<double>();
            Serializer<char>.Storage.Default = new StructSerializer<char>();
        }
    }

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

            if (default(T) != null /* && !RuntimeHelpers.IsReferenceOrContainsReferences<T>()*/)
                return (ISerializer<T>)Activator.CreateInstance(typeof(StructSerializer<>).MakeGenericType(typeof(T)));
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

        public abstract bool IsFixedSize { get; }
        public abstract int CaculateSize(in T value);
        public abstract void Serialize(in T value, Span<byte> storage);
        public abstract void Deserialize(ReadOnlySpan<byte> storage, ref T value);
    }
}
