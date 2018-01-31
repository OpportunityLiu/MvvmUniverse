using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase<T> : ObservableCommandBase, ICommand<T>
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

        /// <summary>
        /// Check whether the command can execute based on
        /// <see cref="ObservableCommandBase.IsEnabled"/> and <see cref="CanExecuteOverride(T)"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
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

        /// <summary>
        /// Will be called by <see cref="CanExecute(T)"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
        protected virtual bool CanExecuteOverride(T parameter) => true;

        void System.Windows.Input.ICommand.Execute(object parameter) => Execute((T)parameter);

        /// <summary>
        /// Execute the <see cref="ICommand{T}"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether execution started or cancelled by
        /// <see cref="CanExecute()"/> or <see cref="Executing"/></returns>
        public bool Execute(T parameter)
        {
            if (!CanExecute(parameter))
                return false;
            if (!OnStarting(parameter))
                return false;

            StartExecution(parameter);
            return true;
        }

        /// <summary>
        /// Start execution with given <paramref name="parameter"/>.
        /// Should call <see cref="OnFinished(ExecutedEventArgs{T})"/> after execution.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        protected abstract void StartExecution(T parameter);

        /// <summary>
        /// Raise <see cref="Executing"/> event.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="Execute(T)"/></param>
        /// <returns>True if executing not cancelled</returns>
        protected virtual bool OnStarting(T parameter)
        {
            var executing = this.Executing;
            if (executing == null)
                return true;
            var eventarg = new ExecutingEventArgs<T>(parameter);
            executing.Invoke(this, eventarg);
            return !eventarg.Cancelled;
        }

        /// <summary>
        /// Raise <see cref="Executed"/> event.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="Execute(T)"/></param>
        /// <param name="e">Event args</param>
        protected virtual void OnFinished(ExecutedEventArgs<T> e)
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

        /// <summary>
        /// Will be raised before execution.
        /// </summary>
        public event ExecutingEventHandler<T> Executing;
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        public event ExecutedEventHandler<T> Executed;
    }
}
