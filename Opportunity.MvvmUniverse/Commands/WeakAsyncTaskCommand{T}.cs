using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class WeakAsyncTaskCommand<T> : WeakAsyncCommand<T>
    {
        internal WeakAsyncTaskCommand(AsyncAction<T> execute, Predicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = new WeakAsyncAction<T>(execute ?? throw new ArgumentNullException(nameof(execute)));
        }

        internal WeakAsyncTaskCommand(WeakAsyncAction<T> execute, WeakPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly WeakAsyncAction<T> execute;

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
