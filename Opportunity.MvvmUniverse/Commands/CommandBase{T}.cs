using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase<T> : ObservableObject, ICommand
    {
        public event EventHandler CanExecuteChanged;

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (Set(ref this.isEnabled, value))
                    RaiseCanExecuteChanged();
            }
        }

        private object tag;
        public object Tag
        {
            get => this.tag;
            set => ForceSet(ref this.tag, value);
        }

        public void RaiseCanExecuteChanged()
        {
            var temp = CanExecuteChanged;
            if (temp == null)
                return;
            DispatcherHelper.BeginInvoke(() => temp(this, EventArgs.Empty));
        }

        bool ICommand.CanExecute(object parameter) => CanExecute(cast(parameter));

        private static T cast(object parameter)
        {
            if (default(T) != null && parameter == null)
                return default(T);
            return (T)parameter;
        }

        public bool CanExecute(T parameter)
        {
            if (!IsEnabled)
                return false;
            return CanExecuteOverride(parameter);
        }

        protected virtual bool CanExecuteOverride(T parameter) => true;

        void ICommand.Execute(object parameter) => Execute(cast(parameter));

        public bool Execute(T parameter)
        {
            if (CanExecute(parameter))
            {
                var executing = this.Executing;
                if (executing != null)
                {
                    var eventarg = new CommandExecutingEventArgs(parameter);
                    executing.Invoke(this, eventarg);
                    if (eventarg.Cancelled)
                        return false;
                }
                var executed = this.Executed;
                var exc = default(Exception);
                try
                {
                    ExecuteImpl(parameter);
                }
                catch (Exception ex)
                {
                    if (executed == null)
                        throw;
                    exc = ex;
                }
                if (executed != null)
                {
                    var eventarg = new CommandExecutedEventArgs(parameter, exc);
                    executed.Invoke(this, eventarg);
                }
                return true;
            }
            return false;
        }

        protected abstract void ExecuteImpl(T parameter);

        public event CommandExecutingEventHandler Executing;
        public event CommandExecutedEventHandler Executed;
    }
}
