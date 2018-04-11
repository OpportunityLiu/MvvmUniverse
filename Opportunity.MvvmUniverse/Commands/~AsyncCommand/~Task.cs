using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="AsyncCommand"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate Task AsyncTaskExecutor(AsyncCommand command);

    internal sealed class AsyncTaskCommand : AsyncCommandImpl
    {
        internal AsyncTaskCommand(AsyncTaskExecutor execute, AsyncPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncTaskExecutor execute;

        protected override IAsyncAction StartExecutionAsync()
        {
            return this.execute.Invoke(this).AsAsyncAction();
        }
    }

    /// <summary>
    /// Execution body of <see cref="AsyncCommand"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="token">Cancellation token of execution.</param>
    public delegate Task CancelableAsyncTaskExecutor(AsyncCommand command, CancellationToken token);

    internal sealed class CancelableAsyncTaskCommand : AsyncCommandImpl
    {
        internal CancelableAsyncTaskCommand(CancelableAsyncTaskExecutor execute, AsyncPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly CancelableAsyncTaskExecutor execute;

        protected override IAsyncAction StartExecutionAsync()
        {
            return AsyncInfo.Run(async token => await this.execute(this, token));
        }
    }
}
