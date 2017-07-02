
using System;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Delegates
{
    public sealed class WeakAsyncAction : WeakDelegate<AsyncAction>
    {
        public WeakAsyncAction(AsyncAction @delegate) : base(@delegate)
        {
        }

        public Task Invoke()
        {
            if (this.IsDelegateOfStaticMethod)
                return this.Delegate.Invoke();
            else
                return (Task)this.DynamicInvoke();
        }
    }

    public sealed class WeakAsyncAction<T> : WeakDelegate<AsyncAction<T>>
    {
        public WeakAsyncAction(AsyncAction<T> @delegate) : base(@delegate)
        {
        }

        public Task Invoke(T obj)
        {
            if (this.IsDelegateOfStaticMethod)
                return this.Delegate.Invoke(obj);
            else
                return (Task)this.DynamicInvoke(obj);
        }
    }
}