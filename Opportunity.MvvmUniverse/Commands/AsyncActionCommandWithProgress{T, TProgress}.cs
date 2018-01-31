using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionCommandWithProgressExecutor<T, TProgress>(AsyncCommandWithProgress<T, TProgress> command, T parameter);

    internal sealed class AsyncActionCommandWithProgress<T, TProgress> : AsyncCommandWithProgress<T, TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionCommandWithProgressExecutor<T, TProgress> execute,
            ProgressMapper<TProgress> progressMapper,
            AsyncCommandPredicate<T> canExecute)
            : base(progressMapper, canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionCommandWithProgressExecutor<T, TProgress> execute;

        protected override async void StartExecution(T parameter)
        {
            try
            {
                var p = this.execute.Invoke(this, parameter);
                p.Progress = (sender, pg) => this.Progress = pg;
                await p;
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
        }
    }
}
