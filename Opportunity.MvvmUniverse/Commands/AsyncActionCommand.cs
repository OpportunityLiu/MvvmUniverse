using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncAction AsyncActionCommandExecutor(AsyncCommand command);

    internal sealed class AsyncActionCommand : AsyncCommand
    {
        public AsyncActionCommand(AsyncActionCommandExecutor execute, AsyncCommandPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionCommandExecutor execute;

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
