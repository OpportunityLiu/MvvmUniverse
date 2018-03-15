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
    /// <typeparam name="THandler">Type of handler handles services.</typeparam>
    public interface IService<THandler>
    {
        /// <summary>
        /// Handlers handles services.
        /// </summary>
        IList<THandler> Handlers { get; }
        /// <summary>
        /// Manually caculates and updates properties depend on <see cref="Handlers"/>.
        /// </summary>
        void UpdateProperties();
    }
}
