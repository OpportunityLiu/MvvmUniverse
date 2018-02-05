using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Storage.Serializers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

            public override int CaculateSize(in T value) => this.caculator(value);
            public override void Deserialize(ReadOnlySpan<byte> storage, ref T value) => value = this.deserializer(storage, value);
            public override void Serialize(in T value, Span<byte> storage) => this.serializer(value, storage);
        }

        internal static object SyncRoot { get; } = new object();

        static Serializer()
        {
            Serializer<string>.Storage.Default = new StringSerializer();
            Serializer<Uri>.Storage.Default = new UriSerializer();
            Serializer<SolidColorBrush>.Storage.Default = new SolidColorBrushSerializer();
            Serializer<int>.Storage.Default = new StructSerializer<int>();
            Serializer<byte>.Storage.Default = new StructSerializer<byte>();
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

            if (default(T) != null && !RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                return (ISerializer<T>)Activator.CreateInstance(typeof(StructSerializer<>).MakeGenericType(typeof(T)));
            if (info.IsNullable)
                return (ISerializer<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(info.NullableUnderlyingType));
            if (tType.IsArray && tType.GetArrayRank() == 1 && !tType.Name.Contains("[*]"))
            {
                var ele = tType.GetElementType();
                return (ISerializer<T>)Activator.CreateInstance(typeof(SZArraySerializer<>).MakeGenericType(ele));
            }

            var tGenericDef = tType.IsConstructedGenericType ? tType.GetGenericTypeDefinition() : null;
            if (tGenericDef == typeof(KeyValuePair<,>))
            {
                return (ISerializer<T>)Activator.CreateInstance(typeof(KeyValuePairSerializer<,>).MakeGenericType(tType.GenericTypeArguments));
            }
            var tIsInterface = tType.IsInterface;
            var tInterfaces = tType.GetInterfaces();

            var tGenericDefInterface = new Type[tInterfaces.Length];
            for (var i = 0; i < tGenericDefInterface.Length; i++)
            {
                var inter = tInterfaces[i];
                tGenericDefInterface[i] = inter.IsConstructedGenericType ? inter.GetGenericTypeDefinition() : null;
            }

            var r = tryGet1(typeof(IList<>), tType, tGenericDef, tInterfaces, tGenericDefInterface, typeof(ListSerializer<,>));
            if (r != null)
                return r;

            r = tryGet1(typeof(ICollection<>), tType, tGenericDef, tInterfaces, tGenericDefInterface, typeof(CollectionSerializer<,>));
            if (r != null)
                return r;

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
                if (Storage.Value != null)
                    return Storage.Value;
                if (Storage.Default == null)
                {
                    lock (SyncRoot)
                    {
                        Storage.Default = createDefault();
                    }
                }
                return Storage.Default;
            }
            set => Storage.Value = value;
        }

        public abstract int CaculateSize(in T value);
        public abstract void Serialize(in T value, Span<byte> storage);
        public abstract void Deserialize(ReadOnlySpan<byte> storage, ref T value);
    }
}
