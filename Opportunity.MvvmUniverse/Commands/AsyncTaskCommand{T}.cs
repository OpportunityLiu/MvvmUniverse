using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    internal sealed class AsyncTaskCommand<T> : AsyncCommand<T>
    {
        internal AsyncTaskCommand(AsyncAction<T> execute, Predicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncAction<T> execute;

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
