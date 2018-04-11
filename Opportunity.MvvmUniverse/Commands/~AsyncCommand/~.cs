﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public static AsyncCommand Create(CancelableAsyncTaskExecutor execute)
            => new CancelableAsyncTaskCommand(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommand"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommand"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Create(CancelableAsyncTaskExecutor execute, AsyncPredicate canExecute)
            => new CancelableAsyncTaskCommand(execute, canExecute);

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
        protected override bool CanExecuteOverride()
            => AsyncCommandHelper.CanExecuteOverride(IsExecuting, ReentrancyHandler);

        /// <summary>
        /// Placeholder for void.
        /// </summary>
        public struct VoidPlaceholder { }

        /// <summary>
        /// Reentrance handling method of async commands.
        /// </summary>
        public IReentrancyHandler<VoidPlaceholder> ReentrancyHandler { get; set; }

        /// <summary>
        /// Indicates whether the command is executing. 
        /// </summary>
        public bool IsExecuting => this.Current != null;

        public override void OnCurrentChanged()
        {
            OnPropertyChanged(EventArgsConst.IsExecutingPropertyChanged);
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Call <see cref="CommandBase.OnStarting()"/>.
        /// </summary>
        /// <returns>True if executing not canceled</returns>
        protected override bool OnStarting()
        {
            if (IsExecuting)
            {
                this.ReentrancyHandler.Enqueue(default, Current);
                return false;
            }
            return base.OnStarting();
        }

        /// <summary>
        /// Call <see cref="CommandBase.OnFinished(IAsyncAction)"/>.
        /// </summary>
        /// <param name="execution">Result of <see cref="CommandBase.StartExecutionAsync()"/>.</param>
        protected override void OnFinished(IAsyncAction execution)
        {
            try
            {
                base.OnFinished(execution);
            }
            finally
            {
                if (ReentrancyHandler.TryDequeue(out _))
                {
                    Execute();
                }
            }
        }
    }
}
