using System;

namespace Opportunity.MvvmUniverse.Delegates
{
    public sealed class WeakAction : WeakDelegate
    {
        public WeakAction(Action @delegate) : base(@delegate)
        {
        }

        public void Invoke()
        {
            this.DynamicInvoke();
        }
    }

    public sealed class WeakAction<T> : WeakDelegate
    {
        public WeakAction(Action<T> @delegate) : base(@delegate)
        {
        }

        public void Invoke(T obj)
        {
            this.DynamicInvoke(obj);
        }
    }
}