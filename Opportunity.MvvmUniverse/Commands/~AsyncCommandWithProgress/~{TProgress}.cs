using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class for commands implements <see cref="IAsyncCommandWithProgress{TProgress}"/>.
    /// </summary>
    /// <typeparam name="TProgress">Type of progress.</typeparam>
    public abstract class AsyncCommandWithProgress<TProgress> : AsyncCommand, IAsyncCommandWithProgress<TProgress>
    {
        #region Factory methods
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommandWithProgress{TProgress}"/>.
        /// </summary>
        /// <param name="execute">Execution body of <see cref="AsyncCommandWithProgress{TProgress}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommandWithProgress{TProgress}"/>.</returns>
        public static AsyncCommandWithProgress<TProgress> Create(AsyncActionWithProgressExecutor<TProgress> execute)
            => new AsyncActionCommandWithProgress<TProgress>(execute, null);
        /// <summary>
        /// Create a new instance of <see cref="AsyncCommandWithProgress{TProgress}"/>.
        /// </summary>
        /// <param name="canExecute">Predicate of <see cref="AsyncCommandWithProgress{TProgress}"/>.</param>
        /// <param name="execute">Execution body of <see cref="AsyncCommandWithProgress{TProgress}"/>.</param>
        /// <returns>A new instance of <see cref="AsyncCommandWithProgress{TProgress}"/>.</returns>
        public static AsyncCommandWithProgress<TProgress> Create(AsyncActionWithProgressExecutor<TProgress> execute, AsyncPredicate canExecute)
            => new AsyncActionCommandWithProgress<TProgress>(execute, canExecute);
        #endregion Factory methods

        private TProgress progress;
        /// <summary>
        /// Progress data of current execution.
        /// </summary>
        public TProgress Progress
        {
            get => this.progress;
            private set
            {
                this.progress = value;
                OnPropertyChanged(ConstPropertyChangedEventArgs.Progress);
            }
        }

        /// <summary>
        /// Call <see cref="AsyncCommand.OnFinished(IAsyncAction)"/> and
        /// set <see cref="Progress"/> to default value.
        /// </summary>
        /// <param name="execution">Result of <see cref="CommandBase.StartExecutionAsync()"/>.</param>
        protected override void OnFinished(IAsyncAction execution)
        {
            try { base.OnFinished(execution); }
            finally { Progress = default; }
        }

        /// <summary>
        /// Set value of <see cref="Progress"/>,
        /// then raise <see cref="ProgressChanged"/> if <see cref="ObservableObject.NotificationSuspending"/> is <see langword="false"/>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnProgress(ProgressChangedEventArgs<TProgress> e)
        {
            if (NotificationSuspending)
            {
                this.progress = e.Progress;
            }
            else
            {
                Progress = e.Progress;
                this.progressChanged.Raise(this, e);
            }
        }

        private readonly DepedencyEvent<ProgressChangedEventHandler<TProgress>, IAsyncCommandWithProgress<TProgress>, ProgressChangedEventArgs<TProgress>> progressChanged = new DepedencyEvent<ProgressChangedEventHandler<TProgress>, IAsyncCommandWithProgress<TProgress>, ProgressChangedEventArgs<TProgress>>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised when <see cref="Progress"/> changed during execution.
        /// </summary>
        public event ProgressChangedEventHandler<TProgress> ProgressChanged
        {
            add => this.progressChanged.Add(value);
            remove => this.progressChanged.Remove(value);
        }
    }
}
