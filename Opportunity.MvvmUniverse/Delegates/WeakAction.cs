using System;

namespace Opportunity.MvvmUniverse.Delegates
{
    public sealed class WeakAction : WeakDelegate<Action>
    {
        public WeakAction(Action @delegate) : base(@delegate)
        {
        }

        public void Invoke()
        {
            if (this.IsDelegateOfStaticMethod)
                this.Delegate.Invoke();
            else
                this.DynamicInvoke();
        }
    }

    public sealed class WeakAction<T> : WeakDelegate<Action<T>>
    {
        public WeakAction(Action<T> @delegate) : base(@delegate)
        {
        }

        public void Invoke(T obj)
        {
            if (this.IsDelegateOfStaticMethod)
                this.Delegate.Invoke(obj);
            else
                this.DynamicInvoke(obj);
        }
    }
}