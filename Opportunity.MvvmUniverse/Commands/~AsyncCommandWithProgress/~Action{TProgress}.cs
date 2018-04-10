using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="AsyncCommandWithProgress{TProgress}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionWithProgressExecutor<TProgress>(AsyncCommandWithProgress<TProgress> command);

    internal sealed class AsyncActionCommandWithProgress<TProgress> : AsyncCommandWithProgress<TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionWithProgressExecutor<TProgress> execute,
            AsyncPredicate canExecute)
        {
            this.canExecute = canExecute;
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncPredicate canExecute;
        private readonly AsyncActionWithProgressExecutor<TProgress> execute;

        protected override IAsyncAction StartExecutionAsync()
        {
            var p = this.execute.Invoke(this);
            var e = ProgressChangedEventArgs<TProgress>.Create(default);
            p.Progress = (sender, pg) => { e.Progress = pg; OnProgress(e.EventArgs); };
            return p.AsTask().AsAsyncAction();
        }

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
