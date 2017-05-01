using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase : ObservableObject
    {
        public event EventHandler CanExecuteChanged;

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                Set(ref this.isEnabled, value);
                RaiseCanExecuteChanged();
            }
        }

        private object tag;
        public object Tag
        {
            get => this.tag;
            set => Set(ref this.tag, value);
        }

        public void RaiseCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }
    }

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

    public class Command<T> : CommandBase, ICommand
    {
        public Command(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            this.execute = new WeakAction<T>(execute);
            if (canExecute != null)
                this.canExecute = new WeakPredicate<T>(canExecute);
        }

        public Command(Action<T> execute) : this(execute, null)
        {
        }

        private readonly WeakAction<T> execute;
        private readonly WeakPredicate<T> canExecute;
        bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);

        public bool CanExecute(T parameter)
        {
            if (!this.IsEnabled)
                return false;
            if (this.canExecute == null)
                return true;
            return this.canExecute.Invoke(parameter);
        }

        void ICommand.Execute(object parameter) => Execute((T)parameter);

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
                this.execute.Invoke(parameter);
        }
    }
}
