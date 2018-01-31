using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase<T> : CommandBaseBase, ICommand<T>
    {
        protected CommandBase() { }

        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            try
            {
                return CanExecute((T)parameter);
            }
            catch { return false; }
        }

        public bool CanExecute(T parameter)
        {
            if (!IsEnabled)
                return false;
            try
            {
                return CanExecuteOverride(parameter);
            }
            catch { return false; }
        }

        protected virtual bool CanExecuteOverride(T parameter) => true;

        void System.Windows.Input.ICommand.Execute(object parameter) => Execute((T)parameter);

        /// <summary>
        /// Execute the <see cref="ICommand"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether execution started or not.</returns>
        public bool Execute(T parameter)
        {
            if (!CanExecute(parameter))
                return false;
            if (!OnStarting(parameter))
                return false;

            StartExecution(parameter);
            return true;
        }

        protected abstract void StartExecution(T parameter);

        protected virtual bool OnStarting(T parameter)
        {
            var executing = this.Executing;
            if (executing == null)
                return true;
            var eventarg = new CommandExecutingEventArgs<T>(parameter);
            executing.Invoke(this, eventarg);
            return !eventarg.Cancelled;
        }

        protected virtual void OnFinished(T parameter)
        {
            var executed = Executed;
            if (executed == null)
                return;
            DispatcherHelper.BeginInvoke(() => executed.Invoke(this, new CommandExecutedEventArgs<T>(parameter, null)));
        }

        protected virtual void OnError(T parameter, Exception error)
        {
            var executed = Executed;
            if (executed == null)
            {
                ThrowUnhandledError(error);
                return;
            }
            DispatcherHelper.BeginInvoke(() => executed.Invoke(this, new CommandExecutedEventArgs<T>(parameter, error)));
        }

        public event CommandExecutingEventHandler<T> Executing;
        public event CommandExecutedEventHandler<T> Executed;
    }
}
