using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;

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
            if (this.executing.InvocationListLength == 0)
                return true;
            var eventarg = new ExecutingEventArgs();
            this.executing.RaiseHasThreadAccessOnly(this, eventarg);
            return !eventarg.Canceled;
        }

        /// <summary>
        /// Raise <see cref="ICommand.Executed"/> event if <see cref="ObservableObject.NotificationSuspending"/> is <see langword="false"/>.
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
            if (this.executed.InvocationListLength == 0 || NotificationSuspending)
            {
                ThrowUnhandledError(error);
                return;
            }
            var args = new ExecutedEventArgs(error);
            var d = DispatcherHelper.Default;
            if (d is null)
                run();
            else
                d.Begin(run);

            async void run()
            {
                this.executed.Raise(this, args);
                if (args.Exception is null)
                    return;
                if (d != null)
                    await d.YieldIdle();
                if (!args.Handled)
                    ThrowUnhandledError(args.Exception);
            }
        }

        private readonly DepedencyEvent<ExecutingEventHandler, ICommand, ExecutingEventArgs> executing
            = new DepedencyEvent<ExecutingEventHandler, ICommand, ExecutingEventArgs>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised before execution, only handlers registed at the same thread of execution starting will receive this event.
        /// </summary>
        public event ExecutingEventHandler Executing
        {
            add => this.executing.Add(value);
            remove => this.executing.Remove(value);
        }

        private readonly DepedencyEvent<ExecutedEventHandler, ICommand, ExecutedEventArgs> executed
            = new DepedencyEvent<ExecutedEventHandler, ICommand, ExecutedEventArgs>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        public event ExecutedEventHandler Executed
        {
            add => this.executed.Add(value);
            remove => this.executed.Remove(value);
        }
    }
}
