using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class Command<T> : CommandBase<T>
    {
        public Command(Action<T> execute, Predicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public Command(Action<T> execute) : this(execute, null)
        {
        }

        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

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
