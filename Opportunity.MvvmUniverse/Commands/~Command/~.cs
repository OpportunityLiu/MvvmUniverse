using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class for sync command without parameter.
    /// </summary>
    public abstract class Command : CommandBase
    {
        #region Factory methods
        /// <summary>
        /// Create a new instance of <see cref="Command"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="Command"/>.</param>
        /// <returns>A new instance of <see cref="Command"/>.</returns>
        public static Command Create(Executor execute) => new CommandImpl(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="Command"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="Command"/>.</param>
        /// <param name="execute">Execution body of <see cref="Command"/>.</param>
        /// <returns>A new instance of <see cref="Command"/>.</returns>
        public static Command Create(Executor execute, Predicate canExecute) => new CommandImpl(execute, canExecute);
        #endregion Factory methods

        /// <summary>
        /// Execution body of <see cref="Command"/>.
        /// </summary>
        protected abstract void ExecuteOverride();

        /// <summary>
        /// Call <see cref="ExecuteOverride()"/>,
        /// returns <see cref="AsyncAction.CreateCompleted()"/> or <see cref="AsyncAction.CreateFault(Exception)"/>.
        /// </summary>
        /// <returns>A completed <see cref="IAsyncAction"/></returns>
        protected sealed override IAsyncAction StartExecutionAsync()
        {
            try
            {
                ExecuteOverride();
                return AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                return AsyncAction.CreateFault(ex);
            }
        }
    }
}
