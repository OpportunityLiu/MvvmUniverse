using System;
using System.Collections.Concurrent;
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
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class AsyncCommand<T> : CommandBase<T>, IAsyncCommand, ICommand<T>
    {
        #region Factory methods
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommand{T}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Create(AsyncTaskExecutor<T> execute)
            => new AsyncTaskCommand<T>(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand{T}"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommand{T}"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommand{T}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Create(AsyncTaskExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new AsyncTaskCommand<T>(execute, canExecute);

        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommand{T}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Create(CancelableAsyncTaskExecutor<T> execute)
            => new CancelableAsyncTaskCommand<T>(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand{T}"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommand{T}"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommand{T}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Create(CancelableAsyncTaskExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new CancelableAsyncTaskCommand<T>(execute, canExecute);

        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand{T}"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommand{T}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Create(AsyncActionExecutor<T> execute)
            => new AsyncActionCommand<T>(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommand{T}"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommand{T}"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommand{T}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Create(AsyncActionExecutor<T> execute, AsyncPredicate<T> canExecute)
            => new AsyncActionCommand<T>(execute, canExecute);
        #endregion Factory methods

        /// <summary>
        /// Check with <see cref="IsExecuting"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
        protected override bool CanExecuteOverride(T parameter)
            => AsyncCommandHelper.CanExecuteOverride(IsExecuting, ReentrancyHandler);

        private IReentrancyHandler<T> reentrancyHandler = Commands.ReentrancyHandler.Disallowed<T>();
        /// <summary>
        /// Reentrance handling method of async commands.
        /// </summary>
        public IReentrancyHandler<T> ReentrancyHandler
        {
            get => this.reentrancyHandler;
            set
            {
                value = value ?? Commands.ReentrancyHandler.Disallowed<T>();
                this.reentrancyHandler.Detach();
                value.Attach(this);
                this.reentrancyHandler = value;
            }
        }

        /// <summary>
        /// Indicates whether the command is executing. 
        /// </summary>
        public bool IsExecuting => this.Current != null;

        /// <summary>
        /// Notify property changed and can execute changed.
        /// </summary>
        public override void OnCurrentChanged()
        {
            OnPropertyChanged(EventArgsConst.IsExecutingPropertyChanged);
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Call <see cref="CommandBase{T}.OnStarting(T)"/>.
        /// </summary>
        /// <param name="parameter">Parameter of execution.</param>
        /// <returns>True if executing not canceled.</returns>
        protected override bool OnStarting(T parameter)
        {
            if (IsExecuting)
            {
                if (this.ReentrancyHandler.Enqueue(parameter))
                {
                    var c = Current;
                    if (c != null && c.Status == AsyncStatus.Started)
                        c.Cancel();
                }
                return false;
            }
            return base.OnStarting(parameter);
        }

        /// <summary>
        /// Call <see cref="CommandBase{T}.OnFinished(IAsyncAction, T)"/>.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="CommandBase{T}.Execute(T)"/>.</param>
        /// <param name="execution">Result of <see cref="CommandBase{T}.StartExecutionAsync(T)"/>.</param>
        protected override void OnFinished(IAsyncAction execution, T parameter)
        {
            try
            {
                base.OnFinished(execution, parameter);
            }
            finally
            {
                if (ReentrancyHandler.TryDequeue(out parameter))
                {
                    Execute(parameter);
                }
            }
        }
    }
}
