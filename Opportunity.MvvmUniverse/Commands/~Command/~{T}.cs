using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class for sync command with parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class Command<T> : CommandBase<T>
    {
        /// <summary>
        /// Execution body of <see cref="Command{T}"/>.
        /// </summary>
        protected abstract void ExecuteOverride(T parameter);

        /// <summary>
        /// Call <see cref="ExecuteOverride(T)"/>,
        /// returns <see cref="AsyncAction.CreateCompleted()"/> or <see cref="AsyncAction.CreateFault(Exception)"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution.</param>
        /// <returns>A completed <see cref="IAsyncAction"/>.</returns>
        protected sealed override IAsyncAction StartExecutionAsync(T parameter)
        {
            try
            {
                ExecuteOverride(parameter);
                return AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                return AsyncAction.CreateFault(ex);
            }
        }
    }
}
