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

    public abstract class Serializer<T> : ISerializer<T>
    {
        public static ISerializer<T> Default { get; } = createDefault();
        private static ISerializer<T> createDefault()
        {
            var info = TypeTraits.Of<T>();

            if (typeof(T) == typeof(string))
                return (ISerializer<T>)(object)new StringSerializer();
            if (!info.CanBeNull && !RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                return (ISerializer<T>)Activator.CreateInstance(typeof(StructSerializer<>).MakeGenericType(typeof(T)));
            if (info.IsNullable)
                return (ISerializer<T>)Activator.CreateInstance(typeof(NullableSerializer<>).MakeGenericType(info.NullableUnderlyingType));
            if (typeof(T) == typeof(Uri))
                return (ISerializer<T>)(object)new UriSerializer();
            if (typeof(T) == typeof(SolidColorBrush))
                return (ISerializer<T>)(object)new SolidColorBrushSerializer();
            if (info.Type.IsArray && info.Type.GetArrayRank() == 1 && !info.Type.Name.Contains("[*]"))
            {
                var ele = info.Type.AsType().GetElementType();
                return (ISerializer<T>)Activator.CreateInstance(typeof(SZArraySerializer<>).MakeGenericType(ele));
            }
            {
                var q = from type in Enumerable.Repeat(info.Type.AsType(), 1).Concat(info.Type.ImplementedInterfaces)
                        where type.GenericTypeArguments.Length == 1 && type == typeof(IList<>).MakeGenericType(type.GenericTypeArguments)
                        select type.GenericTypeArguments[0];
                var ele = q.FirstOrDefault();
                if (ele != null)
                    return (ISerializer<T>)Activator.CreateInstance(typeof(ListSerializer<,>).MakeGenericType(typeof(T), ele));
            }
            {
                var q = from type in Enumerable.Repeat(info.Type.AsType(), 1).Concat(info.Type.ImplementedInterfaces)
                        where type.GenericTypeArguments.Length == 1 && type == typeof(ICollection<>).MakeGenericType(type.GenericTypeArguments)
                        select type.GenericTypeArguments[0];
                var ele = q.FirstOrDefault();
                if (ele != null)
                    return (ISerializer<T>)Activator.CreateInstance(typeof(CollectionSerializer<,>).MakeGenericType(typeof(T), ele));
            }
            return new XmlSerializer<T>();
        }

        public abstract int CaculateSize(in T value);
        public abstract void Serialize(in T value, Span<byte> storage);
        public abstract void Deserialize(ReadOnlySpan<byte> storage, ref T value);

        public static Serializer<T> Create(Func<T, int> sizeCaculator, Action<T, Span<byte>> serializer, Func<ReadOnlySpan<byte>, T, T> deserializer)
        {
            if (sizeCaculator == null)
                throw new ArgumentNullException(nameof(sizeCaculator));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));
            return new DelegateSerializer(sizeCaculator, serializer, deserializer);
        }

        private sealed class DelegateSerializer : Serializer<T>
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
    }
}
