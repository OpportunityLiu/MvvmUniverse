using System;

namespace Opportunity.MvvmUniverse.Delegates
{
    public sealed class WeakFunc<TResult> : WeakDelegate<Func<TResult>>
    {
        public WeakFunc(Func<TResult> @delegate) : base(@delegate)
        {
        }

        public TResult Invoke()
        {
            if (this.IsDelegateOfStaticMethod)
                return this.Delegate.Invoke();
            else
                return (TResult)this.DynamicInvoke();
        }
    }

    public sealed class WeakFunc<T, TResult> : WeakDelegate<Func<T, TResult>>
    {
        public WeakFunc(Func<T, TResult> @delegate) : base(@delegate)
        {
        }

        public TResult Invoke(T obj)
        {
            if (this.IsDelegateOfStaticMethod)
                return this.Delegate.Invoke(obj);
            else
                return (TResult)this.DynamicInvoke(obj);
        }
    }
}