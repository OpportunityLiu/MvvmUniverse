using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate Task AsyncTaskCommandExecutor(AsyncCommand command);

    internal sealed class AsyncTaskCommand : AsyncCommand
    {
        internal AsyncTaskCommand(AsyncTaskCommandExecutor execute, AsyncCommandPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncTaskCommandExecutor execute;

        protected override async void StartExecution()
        {
            try
            {
                await this.execute.Invoke(this);
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
