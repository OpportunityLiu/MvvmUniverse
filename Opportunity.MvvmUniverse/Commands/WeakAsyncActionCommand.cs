using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class WeakAsyncActionCommand : WeakAsyncCommand
    {
        public WeakAsyncActionCommand(Func<IAsyncAction> execute, Func<bool> canExecute)
            : base(canExecute)
        {
            this.execute = new WeakFunc<IAsyncAction>(execute ?? throw new ArgumentNullException(nameof(execute)));
        }

        public WeakAsyncActionCommand(WeakFunc<IAsyncAction> execute, WeakFunc<bool> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly WeakFunc<IAsyncAction> execute;

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
