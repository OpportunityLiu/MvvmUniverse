using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionWithProgressExecutor<TProgress>(AsyncCommandWithProgress<TProgress> command);

    internal sealed class AsyncActionCommandWithProgress<TProgress> : AsyncCommandWithProgress<TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionWithProgressExecutor<TProgress> execute,
            ProgressMapper<TProgress> progressMapper,
            AsyncPredicate canExecute)
            : base(progressMapper, canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionWithProgressExecutor<TProgress> execute;

        protected override Task StartExecutionAsync()
        {
            var p = this.execute.Invoke(this);
            var e = ProgressChangedEventArgs<TProgress>.Create(default);
            p.Progress = (sender, pg) => { e.Progress = pg; OnProgress(e.EventArgs); };
            return p.AsTask();
        }
    }
}
