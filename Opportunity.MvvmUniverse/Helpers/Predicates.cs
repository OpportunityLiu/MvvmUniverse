using System;
using System.Linq;

namespace Opportunity.MvvmUniverse.Helpers
{
    public static class Predicates<T>
    {
        public static Predicate<T> NotNull { get; } = obj => obj != null;
    }
    
    public static class Predicates
    {
        public static Predicate<TSource> SelectNotNull<TSource, TResult>(Func<TSource, TResult> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));
            return t => selector(t) != null;
        }
    }
}