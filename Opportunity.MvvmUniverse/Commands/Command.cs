using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void CommandExecutor(Command command);
    public delegate bool CommandPredicate(Command command);

    public class Command : CommandBase
    {
        public static Command Create(CommandExecutor execute) => new Command(execute, null);
        public static Command Create(CommandExecutor execute, CommandPredicate canExecute) => new Command(execute, canExecute);
        public static Command<T> Create<T>(CommandExecutor<T> execute) => new Command<T>(execute, null);
        public static Command<T> Create<T>(CommandExecutor<T> execute, CommandPredicate<T> canExecute) => new Command<T>(execute, canExecute);

        protected internal Command(CommandExecutor execute, CommandPredicate canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly CommandExecutor execute;
        protected CommandExecutor ExecuteDelegate => this.execute;

        private readonly CommandPredicate canExecute;
        protected CommandPredicate CanExecuteDelegate => this.canExecute;

        protected override bool CanExecuteOverride()
        {
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(this);
        }

        protected override void StartExecution()
        {
            try
            {
                this.execute.Invoke(this);
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
