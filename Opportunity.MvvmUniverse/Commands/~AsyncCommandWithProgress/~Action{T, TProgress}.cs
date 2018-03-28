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
    /// <typeparam name="T">Type of <paramref name="parameter"/>.</typeparam>
    /// <typeparam name="TProgress">Type of original progress.</typeparam>
    /// <param name="command">Caller command.</param>
    /// <param name="parameter">Parameter of current execution.</param>
    /// <param name="progress">Original progress.</param>
    /// <returns>Mapped progress.</returns>
    public delegate double ProgressMapper<T, TProgress>(AsyncCommandWithProgress<T, TProgress> command, T parameter, TProgress progress);

    /// <summary>
    /// Execution body of <see cref="AsyncCommandWithProgress{T, TProgress}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="parameter">Current parameter of execution.</param>
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionWithProgressExecutor<T, TProgress>(AsyncCommandWithProgress<T, TProgress> command, T parameter);

    internal sealed class AsyncActionCommandWithProgress<T, TProgress> : AsyncCommandWithProgress<T, TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionWithProgressExecutor<T, TProgress> execute,
            ProgressMapper<T, TProgress> progressMapper,
            AsyncPredicate<T> canExecute)
        {
            this.canExecute = canExecute;
            this.progressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncPredicate<T> canExecute;
        private readonly ProgressMapper<T, TProgress> progressMapper;
        private readonly AsyncActionWithProgressExecutor<T, TProgress> execute;

        protected override IAsyncAction StartExecutionAsync(T parameter)
        {
            var p = this.execute.Invoke(this, parameter);
            var e = ProgressChangedEventArgs<T, TProgress>.Create(parameter, default);
            p.Progress = (sender, pg) => { e.Progress = pg; OnProgress(e.EventArgs); };
            return p.AsTask().AsAsyncAction();
        }

        protected override double GetNormalizedProgress(T parameter, TProgress progress) => this.progressMapper(this, parameter, progress);

        protected override bool CanExecuteOverride(T parameter)
        {
            if (!base.CanExecuteOverride(parameter))
                return false;
            if (this.canExecute is AsyncPredicate<T> p)
                return p(this, parameter);
            return true;
        }
    }
}
