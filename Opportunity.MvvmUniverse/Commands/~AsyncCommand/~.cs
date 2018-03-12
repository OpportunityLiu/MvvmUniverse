using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="AsyncCommand"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    /// <returns>Whether the command can execute or not.</returns>
    public delegate bool AsyncPredicate(AsyncCommand command);

    /// <summary>
    /// Base class for commands implements <see cref="IAsyncCommand"/>.
    /// </summary>
    public abstract class AsyncCommand : CommandBase, IAsyncCommand
    {
        #region Factory methods
        public static AsyncCommand Create(AsyncTaskExecutor execute)
            => new AsyncTaskCommand(execute, null);
        public static AsyncCommand Create(AsyncTaskExecutor execute, AsyncPredicate canExecute)
            => new AsyncTaskCommand(execute, canExecute);

        public static AsyncCommand Create(AsyncActionExecutor execute)
            => new AsyncActionCommand(execute, null);
        public static AsyncCommand Create(AsyncActionExecutor execute, AsyncPredicate canExecute)
            => new AsyncActionCommand(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncTaskExecutor<T> execute)
            => new AsyncTaskCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncTaskExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new AsyncTaskCommand<T>(execute, canExecute);

        public static AsyncCommand<T> Create<T>(AsyncActionExecutor<T> execute)
            => new AsyncActionCommand<T>(execute, null);
        public static AsyncCommand<T> Create<T>(AsyncActionExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new AsyncActionCommand<T>(execute, canExecute);
        #endregion Factory methods

        /// <summary>
        /// Create new instance of <see cref="AsyncCommand"/>.
        /// </summary>
        /// <param name="canExecute">Value for <see cref="CanExecuteDelegate"/></param>
        protected AsyncCommand(AsyncPredicate canExecute)
        {
            this.CanExecuteDelegate = canExecute;
        }

        /// <summary>
        /// Delegate for <see cref="CanExecuteOverride()"/>.
        /// </summary>
        protected AsyncPredicate CanExecuteDelegate { get; }

        private bool isExecuting = false;
        /// <summary>
        /// Indicates whether the command is executing. 
        /// </summary>
        public bool IsExecuting
        {
            get => this.isExecuting;
            protected set
            {
                if (Set(ref this.isExecuting, value))
                    OnCanExecuteChanged();
            }
        }

        /// <summary>
        /// Check with <see cref="IsExecuting"/> and <see cref="CanExecuteDelegate"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride()
        {
            if (this.IsExecuting)
                return false;
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this);
        }

        /// <summary>
        /// Call <see cref="CommandBase.OnStarting()"/>.
        /// If not be canceled, <see cref="IsExecuting"/> will be set to <see langword="true"/>.
        /// </summary>
        /// <returns>True if executing not canceled</returns>
        protected override bool OnStarting()
        {
            var r = base.OnStarting();
            if (r)
                IsExecuting = true;
            return r;
        }

        /// <summary>
        /// Call <see cref="CommandBase.OnFinished(IAsyncAction)"/> and set <see cref="IsExecuting"/> to <see langword="false"/>.
        /// </summary>
        /// <param name="execution">Result of <see cref="CommandBase.StartExecutionAsync()"/>.</param>
        protected override void OnFinished(IAsyncAction execution)
        {
            IsExecuting = false;
            base.OnFinished(execution);
        }
    }
}
