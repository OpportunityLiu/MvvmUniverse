namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Predicate of <see cref="AsyncCommand"/>.
    /// </summary>
    /// <param name="command">Current command of can execute testing.</param>
    /// <returns>Whether the command can execute or not.</returns>
    public delegate bool AsyncPredicate(AsyncCommand command);

    internal abstract class AsyncCommandImpl : AsyncCommand
    {
        protected AsyncCommandImpl(AsyncPredicate canExecute)
        {
            this.canExecute = canExecute;
        }

        protected override bool CanExecuteOverride()
        {
            if (!base.CanExecuteOverride())
                return false;
            if (this.canExecute is AsyncPredicate p)
                return p(this);
            return true;
        }

        private readonly AsyncPredicate canExecute;
    }
}
