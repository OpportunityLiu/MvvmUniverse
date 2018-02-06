using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate IAsyncAction AsyncActionExecutor<T>(AsyncCommand<T> command, T parameter);

    internal sealed class AsyncActionCommand<T> : AsyncCommand<T>
    {
        public AsyncActionCommand(AsyncActionExecutor<T> execute, AsyncPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionExecutor<T> execute;

        protected override Task StartExecutionAsync(T parameter)
        {
            return this.execute.Invoke(this, parameter).AsTask();
        }
    }
}
