using Opportunity.Helpers;
using Opportunity.MvvmUniverse.Settings.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Settings
{
    public interface ISerializer<T>
    {
        object Serialize(T value);

        T Deserialize(object value);
    }


    public abstract class Serializer<T> : ISerializer<T>
    {
        internal static IReadOnlyCollection<Type> WinRTBaseDataTypes { get; } = new HashSet<Type>
        {
            typeof(void),
            typeof(object),

            typeof(bool),
            typeof(byte),

            typeof(float),
            typeof(double),

            typeof(ushort),
            typeof(short),

            typeof(uint),
            typeof(int),

            typeof(ulong),
            typeof(long),

            typeof(char),
            typeof(string),

            typeof(Guid),

            typeof(DateTimeOffset),
            typeof(TimeSpan),

            typeof(Point),
            typeof(Rect),
            typeof(Size),
        };

        public static ISerializer<T> Default { get; } = createDefault();
        private static ISerializer<T> createDefault()
        {
            var info = TypeTraits.Of<T>();

            if (WinRTBaseDataTypes.Contains(typeof(T)))
                return new EmptySerializer<T>();
            if (info.Type.IsEnum)
                return new EnumSerializer<T>();
            if (typeof(T) == typeof(Uri))
                return (ISerializer<T>)(object)new UriSerializer();
            if (typeof(T) == typeof(DateTime))
                return (ISerializer<T>)(object)new DateTimeSerializer();
            if (typeof(T) == typeof(sbyte))
                return (ISerializer<T>)(object)new SByteSerializer();

            // Unsupported
            return new EmptySerializer<T>();
        }

        public abstract T Deserialize(object value);
        public abstract object Serialize(T value);
    }
}
