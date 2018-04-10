using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class for commands implements <see cref="IAsyncCommandWithProgress{T,TProgress}"/>.
    /// </summary>
    /// <typeparam name="TProgress">Type of progress.</typeparam>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class AsyncCommandWithProgress<T, TProgress> : AsyncCommand<T>, IAsyncCommandWithProgress<T, TProgress>
    {
        #region Factory methods
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.</returns>
        public static AsyncCommandWithProgress<T, TProgress> Create(AsyncActionWithProgressExecutor<T, TProgress> execute)
            => new AsyncActionCommandWithProgress<T, TProgress>(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.</returns>
        public static AsyncCommandWithProgress<T, TProgress> Create(AsyncActionWithProgressExecutor<T, TProgress> execute, AsyncPredicate<T> canExecute)
            => new AsyncActionCommandWithProgress<T, TProgress>(execute, canExecute);
        #endregion Factory methods

        /// <summary>
        /// Progress data of current execution. Will return default value if <see cref="IAsyncCommand.IsExecuting"/> is <see langword="false"/>.
        /// </summary>
        public TProgress Progress { get; private set; }

        private void setProgress(T parameter, TProgress progress)
        {
            Progress = progress;
            OnPropertyChanged(nameof(Progress));
        }

        /// <summary>
        /// Call <see cref="AsyncCommand.OnFinished(IAsyncAction)"/> and
        /// set <see cref="Progress"/> to default value.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="CommandBase{T}.Execute(T)"/>.</param>
        /// <param name="execution">Result of <see cref="CommandBase{T}.StartExecutionAsync(T)"/>.</param>
        protected override void OnFinished(IAsyncAction execution, T parameter)
        {
            try { base.OnFinished(execution, parameter); }
            finally { setProgress(parameter, default); }
        }

        /// <summary>
        /// Set value of <see cref="Progress"/>,
        /// then raise <see cref="ProgressChanged"/> if <see cref="ObservableObject.NotificationSuspending"/> is <see langword="false"/>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnProgress(ProgressChangedEventArgs<T, TProgress> e)
        {
            setProgress(e.Parameter, e.Progress);
            if (!NotificationSuspending)
                this.progressChanged.Raise(this, e);
        }

        private readonly DepedencyEvent<ProgressChangedEventHandler<T, TProgress>, IAsyncCommandWithProgress<T, TProgress>, ProgressChangedEventArgs<T, TProgress>> progressChanged = new DepedencyEvent<ProgressChangedEventHandler<T, TProgress>, IAsyncCommandWithProgress<T, TProgress>, ProgressChangedEventArgs<T, TProgress>>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised when <see cref="Progress"/> changed during execution.
        /// </summary>
        public event ProgressChangedEventHandler<T, TProgress> ProgressChanged
        {
            add => this.progressChanged.Add(value);
            remove => this.progressChanged.Remove(value);
        }
    }
}
