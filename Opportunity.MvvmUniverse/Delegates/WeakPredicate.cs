using System;

namespace Opportunity.MvvmUniverse.Delegates
{
    [WeakReferenceOf(typeof(Predicate<>))]
    public sealed class WeakPredicate<T> : WeakDelegate
    {
        public WeakPredicate(Predicate<T> @delegate) : base(@delegate)
        {
        }

        public bool Invoke(T obj)
        {
            return this.DynamicInvoke<bool>(obj);
        }
    }
}