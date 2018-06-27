using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml;

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

        private readonly static HashSet<string> initViewAssemblies;

        static ViewOfAttribute()
        {
            initViewAssemblies = new HashSet<string>();
            Init(typeof(MvvmPage));
        }

        internal static void Init(Type viewType)
        {
            var assembly = viewType.GetTypeInfo().Assembly;
            if (!initViewAssemblies.Add(assembly.FullName))
                return;
            foreach (var type in assembly.DefinedTypes)
            {
                var attrs = type.GetCustomAttributes<ViewOfAttribute>(true);
                foreach (var item in attrs)
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(item.ViewModelType.TypeHandle);
                }
            }
        }
    }
}
