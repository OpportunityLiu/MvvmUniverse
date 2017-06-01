using Opportunity.MvvmUniverse.Delegates;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class WeakCommand : CommandBase
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

        public bool IsAlive => this.execute.IsAlive && (this.canExecute?.IsAlive == true);

        protected override bool CanExecuteOverride()
        {
            if (!IsAlive)
                return false;
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
