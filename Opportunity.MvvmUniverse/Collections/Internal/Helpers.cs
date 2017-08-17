using System;
using System.Collections;
using System.Collections.Generic;

namespace Opportunity.MvvmUniverse.Collections.Internal
{
    internal static class Helpers
    {
        public static T CastValue<T>(object value)
        {
            if (value == null && default(T) == null)
                return default(T);
            if (value is T v)
                return v;
            throw new ArgumentException("Wrong type of value", nameof(value));
        }

        public static KeyValuePair<TKey, TValue> CastKVP<TKey, TValue>(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is KeyValuePair<TKey, TValue> v)
                return v;
            throw new ArgumentException("Wrong type of value", nameof(value));
        }

        public static TKey CastKey<TKey>(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key is TKey v)
                return v;
            throw new ArgumentException("Wrong type of key", nameof(key));
        }

        public static KeyValuePair<TKey, TValue> CreateKVP<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public static void ThrowForReadOnlyCollection(string parent)
        {
            throw new NotSupportedException($"This collection is a read only view of \"{parent}\".");
        }

        public static T ThrowForReadOnlyCollection<T>(string parent)
        {
            throw new NotSupportedException($"This collection is a read only view of \"{parent}\".");
        }
    }
}