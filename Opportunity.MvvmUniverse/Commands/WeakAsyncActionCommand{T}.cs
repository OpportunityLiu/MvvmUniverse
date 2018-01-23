using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class WeakAsyncActionCommand<T> : WeakAsyncCommand<T>
    {
        public WeakAsyncActionCommand(Func<T, IAsyncAction> execute, Predicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = new WeakFunc<T, IAsyncAction>(execute ?? throw new ArgumentNullException(nameof(execute)));
        }

        public WeakAsyncActionCommand(WeakFunc<T, IAsyncAction> execute, WeakPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly WeakFunc<T, IAsyncAction> execute;

        public override bool IsAlive => this.execute.IsAlive && (this.CanExecuteDelegate?.IsAlive == true);

        protected override async void StartExecution(T parameter)
        {
            this.IsExecuting = true;
            try
            {
                await this.execute.Invoke(parameter);
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }
    }
}
