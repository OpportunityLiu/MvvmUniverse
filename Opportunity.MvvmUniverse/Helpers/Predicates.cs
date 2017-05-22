using System;
using System.Collections.Generic;
using System.Linq;

namespace Opportunity.MvvmUniverse.Helpers
{
    public static class Predicates
    {
        public static Predicate<TSource> NotNull<TSource>()
            where TSource : class
        {
            return obj => obj != null;
        }

        public static Predicate<TSource> SelectNotNull<TSource, TResult>(Func<TSource, TResult> selector)
            where TResult : class
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return t => selector(t) != null;
        }

        public static Predicate<TSource> Equals<TSource>(TSource value)
        {
            return Equals(value, EqualityComparer<TSource>.Default);
        }

        public static Predicate<TSource> SelectEquals<TSource, TResult>(Func<TSource, TResult> selector, TResult value)
        {
            return SelectEquals(selector, value, EqualityComparer<TResult>.Default);
        }

        public static Predicate<TSource> Equals<TSource>(TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            return t => comparer.Equals(t, value);
        }

        public static Predicate<TSource> SelectEquals<TSource, TResult>(Func<TSource, TResult> selector, TResult value, IEqualityComparer<TResult> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return t => comparer.Equals(selector(t), value);
        }
    }
}