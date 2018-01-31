using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate Task AsyncTaskCommandExecutor<T>(AsyncCommand<T> command, T parameter);

    internal sealed class AsyncTaskCommand<T> : AsyncCommand<T>
    {
        internal AsyncTaskCommand(AsyncTaskCommandExecutor<T> execute, AsyncCommandPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncTaskCommandExecutor<T> execute;

        protected override async void StartExecution(T parameter)
        {
            try
            {
                await this.execute.Invoke(this, parameter);
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
        }
    }
}
