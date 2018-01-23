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

    public sealed class WeakAction<TResult> : WeakDelegate<Action<TResult>>
    {
        public WeakAction(Action<TResult> @delegate) : base(@delegate)
        {
        }

        public void Invoke(TResult obj)
        {
            if (this.IsDelegateOfStaticMethod)
                this.Delegate.Invoke(obj);
            else
                this.DynamicInvoke(obj);
        }
    }
}