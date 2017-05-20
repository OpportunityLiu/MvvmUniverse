using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Delegates
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WeakReferenceOfAttribute : Attribute
    {
        public WeakReferenceOfAttribute(Type delegateType)
        {
            this.DelegateType = delegateType;
        }

        public Type DelegateType { get; }
    }
}
