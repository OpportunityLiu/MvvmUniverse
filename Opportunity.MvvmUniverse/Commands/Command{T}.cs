using System;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void Executor<T>(Command<T> command, T parameter);
    public delegate bool Predicate<T>(Command<T> command, T parameter);

    public class Command<T> : CommandBase<T>
    {
        protected internal Command(Executor<T> execute, Predicate<T> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        private readonly Executor<T> execute;
        protected Executor<T> ExecuteDelegate => this.execute;

        private readonly Predicate<T> canExecute;
        protected Predicate<T> CanExecuteDelegate => this.canExecute;

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
                OnFinished(new ExecutedEventArgs<T>(parameter));
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs<T>(parameter, ex));
            }
        }
    }
}
