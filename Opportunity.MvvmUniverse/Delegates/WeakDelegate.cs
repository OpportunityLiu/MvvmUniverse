using System;
using System.Reflection;

namespace Opportunity.MvvmUniverse.Delegates
{
    public class WeakDelegate<T>
        where T : class
    {
        static WeakDelegate()
        {
            var tType = typeof(T);
            var delType = typeof(Delegate);
            if (!tType.GetTypeInfo().IsSubclassOf(delType))
                throw new NotSupportedException($"{tType} is not a delegate type.");
        }

        public WeakDelegate(T @delegate)
        {
            if (@delegate == null)
                throw new ArgumentNullException(nameof(@delegate));
            var d = (Delegate)(object)@delegate;
            if (d.GetInvocationList().Length > 1)
                throw new NotSupportedException("Multicast delegate not supported.");
            if (d.Target != null)
            {
                this.Target = new WeakReference(d.Target);
                this.methodOrDelegate = d.GetMethodInfo();
            }
            else
                this.methodOrDelegate = d;
        }

        protected WeakReference Target { get; }
        protected bool IsDelegateOfStaticMethod => Target == null;
        private readonly object methodOrDelegate;
        protected MethodInfo Method
        {
            get
            {
                try
                {
                    return ((MethodInfo)this.methodOrDelegate);
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }
        }

        protected T Delegate
        {
            get
            {
                try
                {
                    return ((T)this.methodOrDelegate);
                }
                catch (InvalidCastException)
                {
                    return null;
                }
            }
        }

        public bool IsAlive => this.IsDelegateOfStaticMethod ? true : this.Target.IsAlive;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is WeakDelegate<T> o))
                return false;
            if (this.IsDelegateOfStaticMethod)
            {
                if (o.IsDelegateOfStaticMethod)
                    return this.Delegate.Equals(o.Delegate);
                else
                    return false;
            }
            if (o.IsDelegateOfStaticMethod)
                return false;
            return this.Method.Equals(o.Method) && this.Target.Target == o.Target.Target;
        }

        public override int GetHashCode()
        {
            return this.methodOrDelegate.GetHashCode();
        }

        public object DynamicInvoke(params object[] parameters)
        {
            if (IsDelegateOfStaticMethod)
                return ((Delegate)this.methodOrDelegate).DynamicInvoke(parameters);
            else
            {
                var tgtObj = this.Target.Target;
                if (tgtObj == null)
                    throw new InvalidOperationException("Delegate is not alive.");
                return ((MethodInfo)this.methodOrDelegate).Invoke(tgtObj, parameters);
            }
        }
    }
}