using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase<T> : CommandBaseBase, ICommand
    {
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
                throw new InvalidOperationException("Executed is null, can't handle error", error);
            DispatcherHelper.BeginInvoke(() => executed.Invoke(this, new CommandExecutedEventArgs<T>(parameter, error)));
        }

        public event CommandExecutingEventHandler<T> Executing;
        public event CommandExecutedEventHandler<T> Executed;
    }
}
