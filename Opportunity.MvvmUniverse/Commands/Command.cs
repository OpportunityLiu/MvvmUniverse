using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public sealed class Command : CommandBase
    {
        public static Command Create(Action execute) => new Command(execute, null);
        public static Command Create(Action execute, Func<bool> canExecute) => new Command(execute, canExecute);
        public static Command<T> Create<T>(Action<T> execute) => new Command<T>(execute, null);
        public static Command<T> Create<T>(Action<T> execute, Predicate<T> canExecute) => new Command<T>(execute, canExecute);

        internal Command(Action execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly Action execute;
        private readonly Func<bool> canExecute;

        protected override bool CanExecuteOverride()
        {
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke();
        }

        protected override void StartExecution()
        {
            try
            {
                this.execute.Invoke();
                OnFinished();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
