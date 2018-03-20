using System;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Execution body of <see cref="Command{T}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="parameter">Current parameter of execution.</param>
    public delegate void Executor<T>(Command<T> command, T parameter);

    internal sealed class CommandImpl<T> : Command<T>
    {
        internal CommandImpl(Executor<T> execute, Predicate<T> canExecute)
            : base(canExecute)
        {
            this.executor = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly Executor<T> executor;

        protected override void ExecuteOverride(T parameter) => this.executor(this, parameter);
    }
}
