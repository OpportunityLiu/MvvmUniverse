using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Base class of command without parameter.
    /// </summary>
    public abstract class CommandBase : ObservableCommandBase, ICommand
    {
        /// <summary>
        /// Create instance of <see cref="CommandBase"/>.
        /// </summary>
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
            var t = default(IAsyncAction);
            try
            {
                t = StartExecutionAsync() ?? AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                t = AsyncAction.CreateFault(ex);
            }
            if (t.Status != AsyncStatus.Started)
                OnFinished(t);
            else
                t.Completed = (s, _) => OnFinished(s);
            return true;
        }

        /// <summary>
        /// Start execution.
        /// </summary>
        protected abstract IAsyncAction StartExecutionAsync();

        /// <summary>
        /// Raise <see cref="Executing"/> event.
        /// </summary>
        /// <returns>True if executing not canceled</returns>
        protected virtual bool OnStarting()
        {
            var executing = this.Executing;
            if (executing == null)
                return true;
            var eventarg = new ExecutingEventArgs();
            executing.Invoke(this, eventarg);
            return !eventarg.Canceled;
        }

        /// <summary>
        /// Raise <see cref="ICommand.Executed"/> event.
        /// </summary>
        /// <param name="execution">Result of <see cref="StartExecutionAsync()"/></param>
        protected virtual void OnFinished(IAsyncAction execution)
        {
            var error = default(Exception);
            switch (execution.Status)
            {
            case AsyncStatus.Canceled:
                error = new OperationCanceledException();
                break;
            case AsyncStatus.Error:
                error = execution.ErrorCode;
                break;
            }
            var executed = this.Executed;
            if (executed == null)
            {
                ThrowUnhandledError(error);
                return;
            }
            var args = new ExecutedEventArgs(error);
            DispatcherHelper.BeginInvoke(() =>
            {
                executed(this, args);
                if (!args.Handled)
                    ThrowUnhandledError(args.Exception);
            });
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
