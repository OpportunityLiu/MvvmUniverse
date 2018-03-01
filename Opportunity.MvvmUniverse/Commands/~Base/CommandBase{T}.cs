using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                if (default(T) == null)
                    return CanExecute(default);
                else
                    return false;
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

        void System.Windows.Input.ICommand.Execute(object parameter) => Execute((T)parameter);

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

            var t = StartExecutionAsync(parameter) ?? Task.CompletedTask;
            if (t.IsCompleted)
                OnFinished(t, parameter);
            else
                t.ContinueWith((execution, param) => OnFinished(execution, (T)param), parameter);
            return true;
        }


        /// <summary>
        /// Start execution with given <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        protected abstract Task StartExecutionAsync(T parameter);

        /// <summary>
        /// Raise <see cref="Executing"/> event.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="Execute(T)"/></param>
        /// <returns>True if executing not canceled</returns>
        protected virtual bool OnStarting(T parameter)
        {
            var executing = this.Executing;
            if (executing == null)
                return true;
            var eventarg = new ExecutingEventArgs<T>(parameter);
            executing.Invoke(this, eventarg);
            return !eventarg.Canceled;
        }

        /// <summary>
        /// Raise <see cref="Executed"/> event.
        /// </summary>
        /// <param name="parameter">Parameter of <see cref="Execute(T)"/></param>
        /// <param name="execution">result of <see cref="StartExecutionAsync(T)"/></param>
        protected virtual void OnFinished(Task execution, T parameter)
        {
            var error = default(Exception);
            if (execution.IsCanceled)
                error = new TaskCanceledException(execution);
            else if (execution.IsFaulted)
                error = execution.Exception;
            var executed = Executed;
            if (executed == null)
            {
                ThrowUnhandledError(error);
                return;
            }
            var args = new ExecutedEventArgs<T>(parameter, error);
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
        public event ExecutingEventHandler<T> Executing;
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        public event ExecutedEventHandler<T> Executed;
    }
}
