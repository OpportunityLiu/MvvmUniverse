using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase : CommandBaseBase, ICommand
    {
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
                    var eventarg = new CommandExecutingEventArgs();
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
                    var eventarg = new CommandExecutedEventArgs(exc);
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
