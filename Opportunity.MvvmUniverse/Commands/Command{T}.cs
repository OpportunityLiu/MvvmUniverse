using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void CommandExecutor<T>(Command<T> command, T parameter);
    public delegate bool CommandPredicate<T>(Command<T> command, T parameter);

    public class Command<T> : CommandBase<T>
    {
        protected internal Command(CommandExecutor<T> execute, CommandPredicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly CommandExecutor<T> execute;
        protected CommandExecutor<T> ExecuteDelegate => this.execute;

        private readonly CommandPredicate<T> canExecute;
        protected CommandPredicate<T> CanExecuteDelegate => this.canExecute;

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(this, parameter);
        }

        protected override void StartExecution(T parameter)
        {
            try
            {
                this.execute.Invoke(this, parameter);
                OnFinished(parameter);
            }
            catch (Exception ex)
            {
                OnError(parameter, ex);
            }
        }
    }
}
