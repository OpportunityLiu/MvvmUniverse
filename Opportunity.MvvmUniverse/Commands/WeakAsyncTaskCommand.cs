using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class WeakAsyncTaskCommand : WeakAsyncCommand
    {
        internal WeakAsyncTaskCommand(AsyncAction execute, Func<bool> canExecute)
            : base(canExecute)
        {
            this.execute = new WeakAsyncAction(execute ?? throw new ArgumentNullException(nameof(execute)));
        }

        internal WeakAsyncTaskCommand(WeakAsyncAction execute, WeakFunc<bool> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly WeakAsyncAction execute;

        public override bool IsAlive => this.execute.IsAlive && (this.CanExecuteDelegate?.IsAlive == true);

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
