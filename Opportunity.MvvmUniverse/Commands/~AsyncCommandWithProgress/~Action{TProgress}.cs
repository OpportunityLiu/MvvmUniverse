using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Mapping <see cref="ICommandWithProgress{TProgress}.Progress"/> to <see cref="ICommandWithProgress{TProgress}.NormalizedProgress"/>.
    /// </summary>
    /// <typeparam name="TProgress">Type of original progress.</typeparam>
    /// <param name="command">Caller command.</param>
    /// <param name="progress">Original progress.</param>
    /// <returns>Mapped progress.</returns>
    public delegate double ProgressMapper<TProgress>(AsyncCommandWithProgress<TProgress> command, TProgress progress);

    /// <summary>
    /// Execution body of <see cref="AsyncCommandWithProgress{TProgress}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionWithProgressExecutor<TProgress>(AsyncCommandWithProgress<TProgress> command);

    internal sealed class AsyncActionCommandWithProgress<TProgress> : AsyncCommandWithProgress<TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionWithProgressExecutor<TProgress> execute,
            ProgressMapper<TProgress> progressMapper,
            AsyncPredicate canExecute)
        {
            this.canExecute = canExecute;
            this.progressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncPredicate canExecute;
        private readonly ProgressMapper<TProgress> progressMapper;
        private readonly AsyncActionWithProgressExecutor<TProgress> execute;

        protected override IAsyncAction StartExecutionAsync()
        {
            var p = this.execute.Invoke(this);
            var e = ProgressChangedEventArgs<TProgress>.Create(default);
            p.Progress = (sender, pg) => { e.Progress = pg; OnProgress(e.EventArgs); };
            return p.AsTask().AsAsyncAction();
        }

        protected override double GetNormalizedProgress(TProgress progress) => this.progressMapper(this, progress);

        protected override bool CanExecuteOverride()
        {
            if (!base.CanExecuteOverride())
                return false;
            if (this.canExecute is AsyncPredicate p)
                return p(this);
            return true;
        }
    }
}
