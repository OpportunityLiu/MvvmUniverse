using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void Executor(Command command);
    public delegate bool Predicate(Command command);

    public class Command : CommandBase
    {
        public static Command Create(Executor execute) => new Command(execute, null);
        public static Command Create(Executor execute, Predicate canExecute) => new Command(execute, canExecute);
        public static Command<T> Create<T>(Executor<T> execute) => new Command<T>(execute, null);
        public static Command<T> Create<T>(Executor<T> execute, Predicate<T> canExecute) => new Command<T>(execute, canExecute);

        protected internal Command(Executor execute, Predicate canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly Executor execute;
        protected Executor ExecuteDelegate => this.execute;

        private readonly Predicate canExecute;
        protected Predicate CanExecuteDelegate => this.canExecute;

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
                OnFinished(ExecutedEventArgs.Succeed);
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs(ex));
            }
        }
    }
}
