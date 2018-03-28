using System;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="Command"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    public delegate bool Predicate(Command command);
    /// <summary>
    /// Execution body of <see cref="Command"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    public delegate void Executor(Command command);

    internal sealed class CommandImpl : Command
    {
        internal CommandImpl(Executor execute, Predicate canExecute)
        {
            this.canExecute = canExecute;
            this.executor = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly Predicate canExecute;

        protected override bool CanExecuteOverride()
        {
            if (this.canExecute is Predicate p)
                return p(this);
            return true;
        }

        private readonly Executor executor;

        protected override void ExecuteOverride() => this.executor.Invoke(this);
    }
}
