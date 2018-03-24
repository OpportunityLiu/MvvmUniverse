﻿using System;
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
        /// <summary>
        /// Create new instance of <see cref="AsyncCommandWithProgress{TProgress}"/>.
        /// </summary>
        /// <param name="progressMapper">Value for <see cref="ProgressMapper"/>.</param>
        /// <param name="canExecute">Value for <see cref="AsyncCommand.CanExecuteDelegate"/>.</param>
        protected AsyncCommandWithProgress(ProgressMapper<TProgress> progressMapper, AsyncPredicate canExecute)
            : base(canExecute)
        {
            ProgressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
        }

        /// <summary>
        /// Used to map <see cref="Progress"/> to <see cref="NormalizedProgress"/>.
        /// </summary>
        protected ProgressMapper<TProgress> ProgressMapper { get; }

        /// <summary>
        /// Progress data of current execution. Will return default value if <see cref="IAsyncCommand.IsExecuting"/> is <see langword="false"/>.
        /// </summary>
        public TProgress Progress { get; private set; }

        /// <summary>
        /// Normalized progress of current execution, for binding usage.
        /// </summary>
        public double NormalizedProgress { get; private set; }

        private void setProgress(TProgress progress)
        {
            Progress = progress;
            NormalizedProgress = ProgressMapper(this, progress);
            OnPropertyChanged(nameof(Progress), nameof(NormalizedProgress));
        }

        /// <summary>
        /// Call <see cref="AsyncCommand.OnFinished(IAsyncAction)"/> and
        /// set <see cref="Progress"/> to default value,
        /// set <see cref="NormalizedProgress"/> to <c><see cref="ProgressMapper"/>(default).</c>.
        /// </summary>
        /// <param name="execution">Result of <see cref="CommandBase.StartExecutionAsync()"/>.</param>
        protected override void OnFinished(IAsyncAction execution)
        {
            try { base.OnFinished(execution); }
            finally { setProgress(default); }
        }

        /// <summary>
        /// Set value of <see cref="Progress"/> and <see cref="NormalizedProgress"/>,
        /// and raise <see cref="ProgressChanged"/>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnProgress(ProgressChangedEventArgs<TProgress> e)
        {
            setProgress(e.Progress);
            var p = this.ProgressChanged;
            if (p == null)
                return;
            DispatcherHelper.BeginInvoke(() => p.Invoke(this, e));
        }

        /// <summary>
        /// Will be raised when <see cref="Progress"/> changed during execution.
        /// </summary>
        public event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }
}
