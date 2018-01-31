using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionCommandWithProgressExecutor<TProgress>(AsyncCommandWithProgress<TProgress> command);

    internal sealed class AsyncActionCommandWithProgress<TProgress> : AsyncCommandWithProgress<TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionCommandWithProgressExecutor<TProgress> execute,
            ProgressMapper<TProgress> progressMapper,
            AsyncCommandPredicate canExecute)
            : base(progressMapper, canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionCommandWithProgressExecutor<TProgress> execute;

        protected override async void StartExecution()
        {
            try
            {
                var p = this.execute.Invoke(this);
                p.Progress = (sender, pg) => this.Progress = pg;
                await p;
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
