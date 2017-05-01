using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public class Command : CommandBase, ICommand
    {
        public Command(Action execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            this.execute = new WeakAction(execute);
        }

        private readonly WeakAction execute;

        bool ICommand.CanExecute(object parameter)
        {
            return IsEnabled;
        }

        void ICommand.Execute(object parameter) => Execute();

        public void Execute()
        {
            if (IsEnabled)
                this.execute.Invoke();
        }
    }
}
