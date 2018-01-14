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
            try
            {
                return CanExecuteOverride();
            }
            catch { return false; }
        }

        protected virtual bool CanExecuteOverride() => true;

        void ICommand.Execute(object parameter) => Execute();

        /// <summary>
        /// Execute the <see cref="ICommand"/>.
        /// </summary>
        /// <returns>Whether execution started or not.</returns>
        public bool Execute()
        {
            if (!CanExecute())
                return false;
            if (!OnStarting())
                return false;

            StartExecution();
            return true;
        }

        protected abstract void StartExecution();

        protected virtual bool OnStarting()
        {
            var executing = this.Executing;
            if (executing == null)
                return true;
            var eventarg = new CommandExecutingEventArgs();
            executing.Invoke(this, eventarg);
            return !eventarg.Cancelled;
        }

        protected virtual void OnFinished()
        {
            var executed = Executed;
            if (executed == null)
                return;
            DispatcherHelper.BeginInvoke(() => executed.Invoke(this, CommandExecutedEventArgs.Succeed));
        }

        protected virtual void OnError(Exception error)
        {
            var executed = Executed;
            if (executed == null)
            {
                ThrowUnhandledError(error);
                return;
            }
            DispatcherHelper.BeginInvoke(() => executed.Invoke(this, new CommandExecutedEventArgs(error)));
        }

        public event CommandExecutingEventHandler Executing;
        public event CommandExecutedEventHandler Executed;
    }
}
