using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

            CommandHelper.AssertCurrentIsNull(this.current);

            var execution = default(IAsyncAction);
            try
            {
                execution = StartExecutionAsync() ?? AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                execution = AsyncAction.CreateFault(ex);
            }

            CommandHelper.SetCurrent(ref this.current, execution);
            OnCurrentChanged();

            if (execution.Status != AsyncStatus.Started)
                OnFinished(execution);
            else
                execution.Completed = (s, _) => OnFinished(s);
            return true;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IAsyncAction current;
        /// <summary>
        /// Current execution.
        /// </summary>
        public IAsyncAction Current => this.current;

        /// <summary>
        /// Will be called when <see cref="Current"/> changed.
        /// </summary>
        public virtual void OnCurrentChanged()
        {
            this.OnPropertyChanged(ConstPropertyChangedEventArgs.Current);
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
            var ignore = this.executing.RaiseAsync(this, eventarg);
            eventarg.Lock();
            return !eventarg.Canceled;
        }

        /// <summary>
        /// Raise <see cref="ICommand.Executed"/> event if <see cref="ObservableObject.NotificationSuspending"/> is <see langword="false"/>.
        /// </summary>
        /// <param name="execution">Result of <see cref="StartExecutionAsync()"/></param>
        protected virtual void OnFinished(IAsyncAction execution)
        {
            CommandHelper.AssertCurrentEquals(this.current, execution);

            try
            {
                var error = CommandHelper.GetError(execution);
                if (this.executed.InvocationListLength == 0 || NotificationSuspending)
                {
                    DispatcherHelper.ThrowUnhandledError(error);
                    return;
                }
                var args = new ExecutedEventArgs(error);
                var ignore = this.executed.RaiseAsync(this, args);
                CommandHelper.ThrowIfUnhandled(args);
            }
            finally
            {
                CommandHelper.ResetCurrent(ref this.current, execution);
                OnCurrentChanged();
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
