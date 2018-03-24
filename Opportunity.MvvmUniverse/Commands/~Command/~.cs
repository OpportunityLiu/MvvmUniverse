using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="Command"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    public delegate bool Predicate(Command command);

    /// <summary>
    /// Base class for sync command without parameter.
    /// </summary>
    public abstract class Command : CommandBase
    {
        #region Factory methods
        public static Command Create(Executor execute) => new CommandImpl(execute, null);
        public static Command Create(Executor execute, Predicate canExecute) => new CommandImpl(execute, canExecute);
        public static Command<T> Create<T>(Executor<T> execute) => new CommandImpl<T>(execute, null);
        public static Command<T> Create<T>(Executor<T> execute, Predicate<T> canExecute) => new CommandImpl<T>(execute, canExecute);
        #endregion Factory methods

        /// <summary>
        /// Create new instance of <see cref="Command"/>.
        /// </summary>
        /// <param name="canExecute">Value for <see cref="CanExecuteDelegate"/></param>
        protected Command(Predicate canExecute)
        {
            CanExecuteDelegate = canExecute;
        }

        /// <summary>
        /// Delegate for <see cref="CanExecuteOverride()"/>.
        /// </summary>
        protected Predicate CanExecuteDelegate { get; }

        /// <summary>
        /// Check with <see cref="CanExecuteDelegate"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride()
        {
            if (CanExecuteDelegate is Predicate p)
                return p(this);
            return true;
        }

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
