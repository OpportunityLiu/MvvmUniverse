using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncActionWithProgress<TProgress> AsyncActionWithProgressExecutor<T, TProgress>(AsyncCommandWithProgress<T, TProgress> command, T parameter);

    internal sealed class AsyncActionCommandWithProgress<T, TProgress> : AsyncCommandWithProgress<T, TProgress>
    {
        public AsyncActionCommandWithProgress(
            AsyncActionWithProgressExecutor<T, TProgress> execute,
            ProgressMapper<TProgress> progressMapper,
            AsyncPredicate<T> canExecute)
            : base(progressMapper, canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionWithProgressExecutor<T, TProgress> execute;

        protected override async void StartExecution(T parameter)
        {
            try
            {
                var p = this.execute.Invoke(this, parameter);
                var e = ProgressChangedEventArgsFactory.Create(parameter, default(TProgress));
                p.Progress = (sender, pg) => { e.Progress = pg; OnProgress(e.EventArgs); };
                await p;
                OnFinished(new ExecutedEventArgs<T>(parameter));
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs<T>(parameter, ex));
            }
        }
    }
}
