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
            if (@delegate.GetInvocationList().Length > 1)
                throw new NotSupportedException("Multicast delegate not supported.");
            if (@delegate.Target != null)
                this.target = new WeakReference(@delegate.Target);
            this.method = @delegate.GetMethodInfo();
        }

        private WeakReference target;
        private MethodInfo method;

        public bool IsAlive => this.target == null ? true : this.target.IsAlive;

        public object DynamicInvoke(params object[] parameters)
        {
            if (this.target == null)
                return this.method.Invoke(null, parameters);
            else
            {
                var tgtObj = this.target.Target;
                if (tgtObj == null)
                    throw new InvalidOperationException("Delegate is not alive.");
                return this.method.Invoke(tgtObj, parameters);
            }
        }
    }

    internal static class WeakDelegateExtension
    {
        public static TResult DynamicInvoke<TResult>(this WeakDelegate that, params object[] parameters)
        {
            var r = that.DynamicInvoke(parameters);
            if (r is TResult res)
                return res;
            return default(TResult);
        }
    }
}