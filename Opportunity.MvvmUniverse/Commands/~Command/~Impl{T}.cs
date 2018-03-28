using System;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="Command{T}"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    /// <param name="parameter">Current parameter of can execute testing.</param>
    public delegate bool Predicate<T>(Command<T> command, T parameter);
    /// <summary>
    /// Execution body of <see cref="Command{T}"/>.
    /// </summary>
    /// <param name="command">Current command of execution.</param>
    /// <param name="parameter">Current parameter of execution.</param>
    public delegate void Executor<T>(Command<T> command, T parameter);

    internal sealed class CommandImpl<T> : Command<T>
    {
        internal CommandImpl(Executor<T> execute, Predicate<T> canExecute)
        {
            this.canExecute = canExecute;
            this.executor = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        private readonly Predicate<T> canExecute;

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.canExecute is Predicate<T> p)
                return p(this, parameter);
            return true;
        }


        private readonly Executor<T> executor;

        protected override void ExecuteOverride(T parameter) => this.executor(this, parameter);
    }
}
