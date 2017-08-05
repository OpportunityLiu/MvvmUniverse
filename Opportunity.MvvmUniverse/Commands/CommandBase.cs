using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase : ObservableObject, ICommand
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
        bool ICommand.CanExecute(object parameter) => CanExecute();

        public bool CanExecute()
        {
            if (!IsEnabled)
                return false;
            return CanExecuteOverride();
        }

        protected virtual bool CanExecuteOverride() => true;

        void ICommand.Execute(object parameter) => Execute();

        public bool Execute()
        {
            if (CanExecute())
            {
                var executing = this.Executing;
                if (executing != null)
                {
                    var eventarg = new CommandExecutingEventArgs(null);
                    executing.Invoke(this, eventarg);
                    if (eventarg.Cancelled)
                        return false;
                }
                var executed = this.Executed;
                var exc = default(Exception);
                try
                {
                    ExecuteImpl();
                }
                catch (Exception ex)
                {
                    if (executed == null)
                        throw;
                    exc = ex;
                }
                if (executed != null)
                {
                    var eventarg = new CommandExecutedEventArgs(null, exc);
                    executed.Invoke(this, eventarg);
                }
                return true;
            }
            return false;
        }

        protected abstract void ExecuteImpl();

        public event CommandExecutingEventHandler Executing;
        public event CommandExecutedEventHandler Executed;
    }
}
