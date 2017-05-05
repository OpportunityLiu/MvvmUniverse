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

    public sealed class WeakPredicate<T> : WeakDelegate
    {
        public WeakPredicate(Predicate<T> @delegate) : base(@delegate)
        {
        }

        public bool Invoke(T obj)
        {
            return this.DynamicInvoke<bool>(obj);
        }
    }
}