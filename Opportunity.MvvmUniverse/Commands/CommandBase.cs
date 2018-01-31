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
        protected CommandBase() { }

        bool System.Windows.Input.ICommand.CanExecute(object parameter) => CanExecute();

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

        void System.Windows.Input.ICommand.Execute(object parameter) => Execute();

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
            var eventarg = new ExecutingEventArgs();
            executing.Invoke(this, eventarg);
            return !eventarg.Cancelled;
        }

        protected virtual void OnFinished(ExecutedEventArgs e)
        {
            var executed = Executed;
            if (executed == null)
            {
                if (e.Exception != null)
                    ThrowUnhandledError(e.Exception);
                return;
            }
            DispatcherHelper.BeginInvoke(() => executed.Invoke(this, e));
        }

        public event ExecutingEventHandler Executing;
        public event ExecutedEventHandler Executed;
    }
}
