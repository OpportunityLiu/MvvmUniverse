using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncAction AsyncActionExecutor(AsyncCommand command);

    internal sealed class AsyncActionCommand : AsyncCommand
    {
        public AsyncActionCommand(AsyncActionExecutor execute, AsyncPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionExecutor execute;

        protected override async void StartExecution()
        {
            try
            {
                await this.execute.Invoke(this);
                OnFinished(ExecutedEventArgs.Succeed);
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs(ex));
            }
        }
    }
}
