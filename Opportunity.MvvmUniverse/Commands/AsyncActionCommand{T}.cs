using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncAction AsyncActionCommandExecutor<T>(AsyncCommand<T> command, T parameter);

    internal sealed class AsyncActionCommand<T> : AsyncCommand<T>
    {
        public AsyncActionCommand(AsyncActionCommandExecutor<T> execute, AsyncCommandPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionCommandExecutor<T> execute;

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
