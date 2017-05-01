using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class WeakCommand : CommandBaseVoid
    {
        public WeakCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            this.execute = new WeakAction(execute);
            if (canExecute != null)
                this.canExecute = new WeakFunc<bool>(canExecute);
        }

        public WeakCommand(Action execute) : this(execute, null)
        {
        }

        private readonly WeakAction execute;
        private readonly WeakFunc<bool> canExecute;

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
