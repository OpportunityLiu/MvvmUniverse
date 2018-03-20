using System;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="Command"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate void Executor(Command command);

    internal sealed class CommandImpl : Command
    {
        internal CommandImpl(Executor execute, Predicate canExecute)
            : base(canExecute)
        {
            this.executor = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly Executor executor;

        protected override void ExecuteOverride() => this.executor.Invoke(this);
    }
}
