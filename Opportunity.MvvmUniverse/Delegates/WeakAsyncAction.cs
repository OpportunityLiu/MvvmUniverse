
using System;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Delegates
{
    [WeakReferenceOf(typeof(AsyncAction))]
    public sealed class WeakAsyncAction : WeakDelegate
    {
        public WeakAsyncAction(AsyncAction @delegate) : base(@delegate)
        {
        }

        public Task Invoke()
        {
            return this.DynamicInvoke<Task>();
        }
    }

    [WeakReferenceOf(typeof(AsyncAction<>))]
    public sealed class WeakAsyncAction<T> : WeakDelegate
    {
        public WeakAsyncAction(AsyncAction<T> @delegate) : base(@delegate)
        {
        }

        public Task Invoke(T obj)
        {
            return this.DynamicInvoke<Task>(obj);
        }
    }
}