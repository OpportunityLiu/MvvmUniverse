using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class Command : CommandBaseVoid
    {
        public Command(Action execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public Command(Action execute) : this(execute, null)
        {
        }

        private readonly Action execute;
        private readonly Func<bool> canExecute;

        protected override bool CanExecuteOverride()
        {
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }

        protected override void ExecuteImpl()
        {
            this.execute.Invoke();
        }
    }
}
