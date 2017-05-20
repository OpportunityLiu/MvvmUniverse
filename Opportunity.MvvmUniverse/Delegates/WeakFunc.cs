using System;

namespace Opportunity.MvvmUniverse.Delegates
{
    [WeakReferenceOf(typeof(Func<>))]
    public sealed class WeakFunc<TResult> : WeakDelegate
    {
        public WeakFunc(Func<TResult> @delegate) : base(@delegate)
        {
        }

        public TResult Invoke()
        {
            return this.DynamicInvoke<TResult>();
        }
    }

    [WeakReferenceOf(typeof(Func<,>))]
    public sealed class WeakFunc<T, TResult> : WeakDelegate
    {
        public WeakFunc(Func<T, TResult> @delegate) : base(@delegate)
        {
        }

        public TResult Invoke(T obj)
        {
            return this.DynamicInvoke<TResult>(obj);
        }
    }
}