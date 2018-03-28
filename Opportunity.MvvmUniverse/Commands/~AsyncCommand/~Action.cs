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
    public delegate IAsyncAction AsyncActionExecutor(AsyncCommand command);

    internal sealed class AsyncActionCommand : AsyncCommandImpl
    {
        public AsyncActionCommand(AsyncActionExecutor execute, AsyncPredicate canExecute)
            : base(canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly AsyncActionExecutor execute;

        protected override IAsyncAction StartExecutionAsync()
        {
            return this.execute.Invoke(this);
        }
    }
}
