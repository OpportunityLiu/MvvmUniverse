using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Services
{
    /// <summary>
    /// Interface for sevices.
    /// </summary>
    /// <typeparam name="TService">Type of services, the type that implements this interface.</typeparam>
    /// <typeparam name="THandler">Type of handler handles services.</typeparam>
    public interface IService<TService, THandler>
        where TService : class, IService<TService, THandler>
        where THandler : IServiceHandler<TService>
    {
        /// <summary>
        /// Handlers handles services.
        /// </summary>
        ServiceHandlerCollection<TService, THandler> Handlers { get; }
        /// <summary>
        /// Manually caculates and updates properties depend on <see cref="Handlers"/>.
        /// </summary>
        void UpdateProperties();
    }
}
