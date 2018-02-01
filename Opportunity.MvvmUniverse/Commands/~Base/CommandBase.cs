using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class CommandBase : ObservableCommandBase, ICommand
    {
        protected CommandBase() { }

        bool System.Windows.Input.ICommand.CanExecute(object parameter) => CanExecute();

        /// <summary>
        /// Check whether the command can execute based on
        /// <see cref="ObservableCommandBase.IsEnabled"/> and <see cref="CanExecuteOverride()"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
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

        /// <summary>
        /// Will be called by <see cref="CanExecute()"/>.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
        protected virtual bool CanExecuteOverride() => true;

        void System.Windows.Input.ICommand.Execute(object parameter) => Execute();

        /// <summary>
        /// Execute the <see cref="ICommand"/>.
        /// </summary>
        /// <returns>Whether execution started or cancelled by
        /// <see cref="CanExecute()"/> or <see cref="Executing"/></returns>
        public bool Execute()
        {
            if (!CanExecute())
                return false;
            if (!OnStarting())
                return false;

            StartExecution();
            return true;
        }

        /// <summary>
        /// Start execution.
        /// Should call <see cref="OnFinished(ExecutedEventArgs)"/> after execution.
        /// </summary>
        protected abstract void StartExecution();

        /// <summary>
        /// Raise <see cref="Executing"/> event.
        /// </summary>
        /// <returns>True if executing not cancelled</returns>
        protected virtual bool OnStarting()
        {
            var executing = this.Executing;
            if (executing == null)
                return true;
            var eventarg = new ExecutingEventArgs();
            executing.Invoke(this, eventarg);
            return !eventarg.Cancelled;
        }

        /// <summary>
        /// Raise <see cref="Executed"/> event.
        /// </summary>
        /// <param name="e">Event args</param>
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

        /// <summary>
        /// Will be raised before execution.
        /// </summary>
        public event ExecutingEventHandler Executing;
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        public event ExecutedEventHandler Executed;
    }
}
