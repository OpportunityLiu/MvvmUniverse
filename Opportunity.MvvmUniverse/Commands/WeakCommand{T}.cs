using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class WeakCommand<T> : CommandBase<T>
    {
        public WeakCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            this.execute = new WeakAction<T>(execute);
            if (canExecute != null)
                this.canExecute = new WeakPredicate<T>(canExecute);
        }

        public WeakCommand(Action<T> execute) : this(execute, null)
        {
        }

        private readonly WeakAction<T> execute;
        private readonly WeakPredicate<T> canExecute;

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }

        protected override void ExecuteImpl(T parameter)
        {
            this.execute.Invoke(parameter);
        }
    }
}
