using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate Task AsyncTaskExecutor<T>(AsyncCommand<T> command, T parameter);

    internal sealed class AsyncTaskCommand<T> : AsyncCommand<T>
    {
        internal AsyncTaskCommand(AsyncTaskExecutor<T> execute, AsyncPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncTaskExecutor<T> execute;

        protected override async void StartExecution(T parameter)
        {
            try
            {
                await this.execute.Invoke(this, parameter);
                OnFinished(new ExecutedEventArgs<T>(parameter, null));
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs<T>(parameter, ex));
            }
        }
    }
}
