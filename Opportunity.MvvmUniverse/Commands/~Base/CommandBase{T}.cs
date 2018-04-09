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
    /// Base class of command with parameter of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class CommandBase<T> : ObservableCommandBase, ICommand<T>
    {
        /// <summary>
        /// Create instance of <see cref="CommandBase{T}"/>.
        /// </summary>
        protected CommandBase() { }

        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            if (parameter is null)
            {
                if (default(T) != null)
                    return false;
                else
                    return CanExecute(default);
            }
            if (parameter is T t)
                return CanExecute(t);
            return false;
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

        void System.Windows.Input.ICommand.Execute(object parameter)
        {
            if (parameter is null)
            {
                if (default(T) != null)
                    return;
                Execute(default);
            }
            else if (parameter is T t)
                Execute(t);
        }

        /// <summary>
        /// Execute the <see cref="ICommand{T}"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether execution started or cancelled by
        /// <see cref="CanExecute(T)"/> or <see cref="Executing"/></returns>
        public bool Execute(T parameter)
        {
            if (!CanExecute(parameter))
                return false;
            if (!OnStarting(parameter))
                return false;

            var t = default(IAsyncAction);
            try
            {
                t = StartExecutionAsync(parameter) ?? AsyncAction.CreateCompleted();
            }
            catch (Exception ex)
            {
                t = AsyncAction.CreateFault(ex);
            }
            if (t.Status != AsyncStatus.Started)
                OnFinished(t, parameter);
            else
                t.Completed = (s, _) => OnFinished(s, parameter);
            return true;
        }


        /// <summary>
        /// Start execution with given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        protected abstract IAsyncAction StartExecutionAsync(T parameter);

        /// <summary>
        /// Raise <see cref="Executing"/> event.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="Execute(T)"/></param>
        /// <returns>True if executing not canceled</returns>
        protected virtual bool OnStarting(T parameter)
        {
            if (this.executing.InvocationListLength == 0)
                return true;
            var eventarg = new ExecutingEventArgs<T>(parameter);
            this.executing.RaiseHasThreadAccessOnly(this, eventarg);
            return !eventarg.Canceled;
        }

        /// <summary>
        /// Raise <see cref="Executed"/> event.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="Execute(T)"/></param>
        /// <param name="execution">Result of <see cref="StartExecutionAsync(T)"/></param>
        protected virtual void OnFinished(IAsyncAction execution, T parameter)
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
            if (this.executed.InvocationListLength == 0)
            {
                ThrowUnhandledError(error);
                return;
            }
            var args = new ExecutedEventArgs<T>(parameter, error);
            var d = CoreApplication.MainView?.Dispatcher;
            if (d is null)
            {
                this.executed.Raise(this, args);
                if (args.Exception is null)
                    return;
                if (!args.Handled)
                    ThrowUnhandledError(args.Exception);
            }
            else
                d.Begin(async () =>
                {
                    this.executed.Raise(this, args);
                    if (args.Exception is null)
                        return;
                    await d.YieldIdle();
                    if (!args.Handled)
                        ThrowUnhandledError(args.Exception);
                });
        }

        private readonly DepedencyEvent<ExecutingEventHandler<T>, ICommand<T>, ExecutingEventArgs<T>> executing
            = new DepedencyEvent<ExecutingEventHandler<T>, ICommand<T>, ExecutingEventArgs<T>>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised before execution.
        /// </summary>
        public event ExecutingEventHandler<T> Executing
        {
            add => this.executing.Add(value);
            remove => this.executing.Remove(value);
        }

        private readonly DepedencyEvent<ExecutedEventHandler<T>, ICommand<T>, ExecutedEventArgs<T>> executed
            = new DepedencyEvent<ExecutedEventHandler<T>, ICommand<T>, ExecutedEventArgs<T>>((h, s, e) => h(s, e));
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        public event ExecutedEventHandler<T> Executed
        {
            add => this.executed.Add(value);
            remove => this.executed.Remove(value);
        }
    }
}
