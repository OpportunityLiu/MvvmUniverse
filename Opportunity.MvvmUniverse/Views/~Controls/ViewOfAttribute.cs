using System;
using System.Collections.Generic;
using System.Reflection;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Mark view model of <see cref="MvvmPage"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ViewOfAttribute : Attribute
    {
        /// <summary>
        /// Create new instance of <see cref="ViewOfAttribute"/>.
        /// </summary>
        /// <param name="viewModelType">Type of view model, <see cref="MvvmPage.ViewModel"/> should be of this type.</param>
        public ViewOfAttribute(Type viewModelType)
        {
            this.ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
        }

        /// <summary>
        /// Type of view model, <see cref="MvvmPage.ViewModel"/> should be of this type.
        /// </summary>
        public Type ViewModelType { get; }

        private readonly static HashSet<RuntimeTypeHandle> initViewTypes = new HashSet<RuntimeTypeHandle>();

        internal static void Init(Type viewType)
        {
            var handle = viewType.TypeHandle;
            if (!initViewTypes.Add(handle))
                return;
            var attrs = viewType.GetTypeInfo().GetCustomAttributes<ViewOfAttribute>(true);
            foreach (var item in attrs)
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(item.ViewModelType.TypeHandle);
            }
        }
    }
}
