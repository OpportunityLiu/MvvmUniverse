using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class AsyncActionCommand : AsyncCommand
    {
        public AsyncActionCommand(Func<IAsyncAction> execute, Func<bool> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly Func<IAsyncAction> execute;

        protected override async void StartExecution()
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke();
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
