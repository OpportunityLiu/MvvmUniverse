using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class Command : CommandBase, ICommand
    {
        public Command(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            this.execute = new WeakAction(execute);
            if (canExecute != null)
                this.canExecute = new WeakFunc<bool>(canExecute);
        }

        public Command(Action execute) : this(execute, null)
        {
        }

        private readonly WeakAction execute;
        private readonly WeakFunc<bool> canExecute;

        bool ICommand.CanExecute(object parameter) => CanExecute();

        public bool CanExecute()
        {
            if (!IsEnabled)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }

        void ICommand.Execute(object parameter) => Execute();

        public bool Execute()
        {
            if (CanExecute())
            {
                this.execute.Invoke();
                return true;
            }
            return false;
        }
    }
}
