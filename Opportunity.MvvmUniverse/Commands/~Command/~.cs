using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="Command"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate void Executor(Command command);
    /// <summary>
    /// Predicate of <see cref="Command"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    public delegate bool Predicate(Command command);

    public class Command : CommandBase
    {
        #region Factory methods
        public static Command Create(Executor execute) => new Command(execute, null);
        public static Command Create(Executor execute, Predicate canExecute) => new Command(execute, canExecute);
        public static Command<T> Create<T>(Executor<T> execute) => new Command<T>(execute, null);
        public static Command<T> Create<T>(Executor<T> execute, Predicate<T> canExecute) => new Command<T>(execute, canExecute);
        #endregion Factory methods

        protected internal Command(Executor execute, Predicate canExecute)
        {
            this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
            this.CanExecuteDelegate = canExecute;
        }

        protected Executor ExecuteDelegate { get; }
        protected Predicate CanExecuteDelegate { get; }

        /// <summary>
        /// Check with <see cref="CanExecuteDelegate"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride()
        {
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this);
        }

        /// <summary>
        /// Returns <see cref="AsyncAction.CreateCompleted()"/> or <see cref="AsyncAction.CreateFault(Exception)"/>.
        /// </summary>
        /// <returns>A completed <see cref="IAsyncAction"/></returns>
        protected override IAsyncAction StartExecutionAsync()
        {
            try
            {
                this.ExecuteDelegate.Invoke(this);
                return AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                return AsyncAction.CreateFault(ex);
            }
        }
    }
}
