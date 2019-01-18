using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse.Services
{
    /// <summary>
    /// Base class for services that available for single ui thread.
    /// </summary>
    /// <typeparam name="TService">Type of services, the type that derives this class.</typeparam>
    /// <typeparam name="THandler">Type of handler handles services.</typeparam>
    public abstract class DependencyServiceBase<TService, THandler> : DependencyObject, IService<TService, THandler>
        where TService : DependencyServiceBase<TService, THandler>
        where THandler : IServiceHandler<TService>
    {
        /// <summary>
        /// Create new instance of <see cref="DependencyServiceBase{TService, THandler}"/>.
        /// </summary>
        protected DependencyServiceBase()
        {
            this.Handlers = new ServiceHandlerCollection<TService, THandler>((TService)this);
        }

        /// <summary>
        /// Handlers of this service.
        /// </summary>
        public ServiceHandlerCollection<TService, THandler> Handlers { get; }

        /// <summary>
        /// Implement of <see cref="IService{TService, THandler}.UpdateProperties()"/>
        /// </summary>
        protected virtual void UpdatePropertiesOverride() { }

        /// <summary>
        /// Manually caculates and updates properties depend on <see cref="Handlers"/>.
        /// </summary>
        public void UpdateProperties() => UpdatePropertiesOverride();
    }

    /// <summary>
    /// Base class for services that available for all threads.
    /// </summary>
    /// <typeparam name="TService">Type of services, the type that derives this class.</typeparam>
    /// <typeparam name="THandler">Type of handler handles services.</typeparam>
    public abstract class ServiceBase<TService, THandler> : ObservableObject, IService<TService, THandler>
        where TService : ServiceBase<TService, THandler>
        where THandler : IServiceHandler<TService>
    {
        /// <summary>
        /// Create new instance of <see cref="ServiceBase{TService, THandler}"/>.
        /// </summary>
        protected ServiceBase()
        {
            this.Handlers = new ServiceHandlerCollection<TService, THandler>((TService)this);
        }

        /// <summary>
        /// Handlers of this service.
        /// </summary>
        public ServiceHandlerCollection<TService, THandler> Handlers { get; }

        /// <summary>
        /// Implement of <see cref="IService{TService, THandler}.UpdateProperties()"/>
        /// </summary>
        protected virtual void UpdatePropertiesOverride() { }

        /// <summary>
        /// Manually caculates and updates properties depend on <see cref="Handlers"/>.
        /// </summary>
        public void UpdateProperties() => UpdatePropertiesOverride();
    }
}
