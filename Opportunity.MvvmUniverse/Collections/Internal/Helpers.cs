using System;
using System.Collections;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections.Internal
{
    internal static class Helpers
    {
        public static bool TryCastValue<T>(object value, out T item)
        {
            if (value is null)
            {
                item = default;
                return (default(T) == null);
            }
            if (value is T)
            {
                item = (T)value;
                return true;
            }
            item = default;
            return false;
        }

        public static T CastValue<T>(object value) => CastValue<T>(value, nameof(value));

        public static T CastValue<T>(object value, string paramName)
        {
            if (value is null)
            {
                if (default(T) == null)
                    return default;
                throw new ArgumentNullException(paramName);
            }
            if (value is T v)
                return v;
            throw new ArgumentException($"Wrong type of value, {typeof(T)} expected.", paramName);
        }

        public static KeyValuePair<TKey, TValue> CastKVP<TKey, TValue>(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (value is KeyValuePair<TKey, TValue> v)
                return v;
            throw new ArgumentException($"Wrong type of value, {typeof(KeyValuePair<TKey, TValue>)} expected.", nameof(value));
        }

        public static TKey CastKey<TKey>(object key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (key is TKey v)
                return v;
            throw new ArgumentException($"Wrong type of key, {typeof(TKey)} expected.", nameof(key));
        }

        public static KeyValuePair<TKey, TValue> CreateKVP<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public static void ThrowForFixedSizeCollection()
        {
            throw new NotSupportedException("This collection is fixed size.");
        }

        public static T ThrowForFixedSizeCollection<T>()
        {
            throw new NotSupportedException("This collection is fixed size.");
        }

        public static void ThrowForReadOnlyCollection()
        {
            throw new NotSupportedException("This collection is read only.");
        }

        public static void ThrowForReadOnlyCollection(object parent)
        {
            throw new NotSupportedException($"This collection is a read only view of \"{parent}\".");
        }

        public static T ThrowForReadOnlyCollection<T>(object parent)
        {
            throw new NotSupportedException($"This collection is a read only view of \"{parent}\".");
        }
    }
}