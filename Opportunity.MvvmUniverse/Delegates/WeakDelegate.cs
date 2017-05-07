using System;
using System.Reflection;

namespace Opportunity.MvvmUniverse.Delegates
{
    public class WeakDelegate
    {
        public WeakDelegate(Delegate @delegate)
        {
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));
            if (@delegate.Target != null)
                this.target = new WeakReference(@delegate.Target);
            this.method = @delegate.GetMethodInfo();
        }

        private WeakReference target;
        private MethodInfo method;

        public bool IsAlive => this.target == null ? true : this.target.IsAlive;

        public object DynamicInvoke(params object[] parameters)
        {
            if (!IsAlive)
                return null;
            return this.method.Invoke(this.target?.Target, parameters);
        }

        public TResult DynamicInvoke<TResult>(params object[] parameters)
        {
            var r = DynamicInvoke(parameters);
            if (r is TResult res)
                return res;
            return default(TResult);
        }
    }
}