using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Services
{
    /// <summary>
    /// Service handler for <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TService">Type of service to handle.</typeparam>
    public interface IServiceHandler<in TService>
    {
        /// <summary>
        /// Will be called when adding to <see cref="IService{TService, THandler}.Handlers"/>.
        /// </summary>
        /// <param name="service">The <typeparamref name="TService"/> of which is adding to.</param>
        void OnAdd(TService service);
        /// <summary>
        /// Will be called when removing from <see cref="IService{TService, THandler}.Handlers"/>.
        /// </summary>
        /// <param name="service">The <typeparamref name="TService"/> of which is removing from.</param>
        void OnRemove(TService service);
    }
}
