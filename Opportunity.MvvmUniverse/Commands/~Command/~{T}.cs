using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="Command{T}"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    /// <param name="parameter">Current parameter of can execute testing.</param>
    public delegate bool Predicate<T>(Command<T> command, T parameter);

    /// <summary>
    /// Base class for sync command with parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class Command<T> : CommandBase<T>
    {
        /// <summary>
        /// Create new instance of <see cref="Command"/>.
        /// </summary>
        /// <param name="canExecute">Value for <see cref="CanExecuteDelegate"/></param>
        protected Command(Predicate<T> canExecute)
        {
            this.CanExecuteDelegate = canExecute;
        }

        /// <summary>
        /// Delegate for <see cref="CanExecuteOverride(T)"/>.
        /// </summary>
        protected Predicate<T> CanExecuteDelegate { get; }

        /// <summary>
        /// Check with <see cref="CanExecuteDelegate"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution.</param>
        /// <returns>Whether the command can execute or not.</returns>
        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this, parameter);
        }

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
                this.ExecuteOverride(parameter);
                return AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                return AsyncAction.CreateFault(ex);
            }
        }
    }
}
