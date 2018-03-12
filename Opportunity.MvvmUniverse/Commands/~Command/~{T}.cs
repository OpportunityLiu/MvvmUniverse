using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="Command{T}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="parameter">Current parameter of execution.</param>
    public delegate void Executor<T>(Command<T> command, T parameter);
    /// <summary>
    /// Predicate of <see cref="Command{T}"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    /// <param name="parameter">Current parameter of can execute testing.</param>
    public delegate bool Predicate<T>(Command<T> command, T parameter);

    public class Command<T> : CommandBase<T>
    {
        protected internal Command(Executor<T> execute, Predicate<T> canExecute)
        {
            this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
            this.CanExecuteDelegate = canExecute;
        }

        protected Executor<T> ExecuteDelegate { get; }
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
        /// Returns <see cref="AsyncAction.CreateCompleted()"/> or <see cref="AsyncAction.CreateFault(Exception)"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution.</param>
        /// <returns>A completed <see cref="IAsyncAction"/>.</returns>
        protected override IAsyncAction StartExecutionAsync(T parameter)
        {
            try
            {
                this.ExecuteDelegate.Invoke(this, parameter);
                return AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                return AsyncAction.CreateFault(ex);
            }
        }
    }
}
