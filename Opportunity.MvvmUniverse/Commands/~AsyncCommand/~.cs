using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class for commands implements <see cref="IAsyncCommand"/>.
    /// </summary>
    public abstract class AsyncCommand : CommandBase, IAsyncCommand, ICommand
    {
        #region Factory methods
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommand"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Create(AsyncTaskExecutor execute)
            => new AsyncTaskCommand(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommand"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommand"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Create(AsyncTaskExecutor execute, AsyncPredicate canExecute)
            => new AsyncTaskCommand(execute, canExecute);

        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommand"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Create(AsyncActionExecutor execute)
            => new AsyncActionCommand(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommand"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommand"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Create(AsyncActionExecutor execute, AsyncPredicate canExecute)
            => new AsyncActionCommand(execute, canExecute);
        #endregion Factory methods

        /// <summary>
        /// Check with <see cref="IsExecuting"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not.</returns>
        protected override bool CanExecuteOverride() => !this.isExecuting;

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
