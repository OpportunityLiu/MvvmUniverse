using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="AsyncCommand"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate Task AsyncTaskExecutor(AsyncCommand command);

    internal sealed class AsyncTaskCommand : AsyncCommand
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
}
