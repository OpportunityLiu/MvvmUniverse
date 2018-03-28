using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="AsyncCommand{T}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="parameter">Current parameter of execution.</param>
    public delegate Task AsyncTaskExecutor<T>(AsyncCommand<T> command, T parameter);

    internal sealed class AsyncTaskCommand<T> : AsyncCommandImpl<T>
    {
        internal AsyncTaskCommand(AsyncTaskExecutor<T> execute, AsyncPredicate<T> canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncTaskExecutor<T> execute;

        protected override IAsyncAction StartExecutionAsync(T parameter)
        {
            return this.execute.Invoke(this, parameter).AsAsyncAction();
        }
    }
}
