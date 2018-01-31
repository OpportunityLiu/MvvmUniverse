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

        protected override async void StartExecution()
        {
            try
            {
                var p = this.execute.Invoke(this);
                var e = ProgressChangedEventArgsFactory.Create(default(TProgress));
                p.Progress = (sender, pg) => { e.Progress = pg; OnProgress(e.EventArgs); };
                await p;
                OnFinished(ExecutedEventArgs.Succeed);
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs(ex));
            }
        }
    }
}
