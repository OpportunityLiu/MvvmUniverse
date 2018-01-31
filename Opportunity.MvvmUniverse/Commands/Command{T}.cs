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
            this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
            this.CanExecuteDelegate = canExecute;
        }

        protected Executor<T> ExecuteDelegate { get; }
        protected Predicate<T> CanExecuteDelegate { get; }

        protected override bool CanExecuteOverride(T parameter)
        {
            if (this.CanExecuteDelegate == null)
                return true;
            return this.CanExecuteDelegate.Invoke(this, parameter);
        }

        protected override void StartExecution(T parameter)
        {
            try
            {
                this.ExecuteDelegate.Invoke(this, parameter);
                OnFinished(new ExecutedEventArgs<T>(parameter));
            }
            catch (Exception ex)
            {
                OnFinished(new ExecutedEventArgs<T>(parameter, ex));
            }
        }
    }
}
