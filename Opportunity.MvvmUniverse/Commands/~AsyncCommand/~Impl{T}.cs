namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="AsyncCommand{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    /// <param name="command">Current command of can execute testing.</param>
    /// <param name="parameter">Current parameter of can execute testing.</param>
    /// <returns>Whether the command can execute or not.</returns>
    public delegate bool AsyncPredicate<T>(AsyncCommand<T> command, T parameter);

    internal abstract class AsyncCommandImpl<T> : AsyncCommand<T>
    {
        protected AsyncCommandImpl(AsyncPredicate<T> canExecute)
        {
            this.canExecute = canExecute;
        }

        protected override bool CanExecuteOverride(T parameter)
        {
            if (!base.CanExecuteOverride(parameter))
                return false;
            if (this.canExecute is AsyncPredicate<T> p)
                return p(this, parameter);
            return true;
        }

        private readonly AsyncPredicate<T> canExecute;
    }
}
