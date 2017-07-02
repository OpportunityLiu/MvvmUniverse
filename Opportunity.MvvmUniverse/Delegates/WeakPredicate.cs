using System;

namespace Opportunity.MvvmUniverse.Delegates
{
    public sealed class WeakPredicate<T> : WeakDelegate<Predicate<T>>
    {
        public WeakPredicate(Predicate<T> @delegate) : base(@delegate)
        {
        }

        public bool Invoke(T obj)
        {
            if (this.IsDelegateOfStaticMethod)
                return this.Delegate.Invoke(obj);
            else
                return (bool)this.DynamicInvoke(obj);
        }
    }
}