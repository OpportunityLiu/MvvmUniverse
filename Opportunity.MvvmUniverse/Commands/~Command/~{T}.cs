using System;
using System.Threading.Tasks;
using System.Windows.Input;

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
        /// <param name="parameter">Parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this, parameter);
        }

        /// <summary>
        /// Returns <see cref="Task.CompletedTask"/> or <see cref="Task.FromException(Exception)"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>A completed <see cref="Task"/></returns>
        protected override Task StartExecutionAsync(T parameter)
        {
            try
            {
                this.ExecuteDelegate.Invoke(this, parameter);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
