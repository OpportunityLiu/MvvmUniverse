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
    /// Handler for <see cref="ICommand.Executed"/> event.
    /// </summary>
    /// <param name="sender"><see cref="ICommand"/> that raise the event.</param>
    /// <param name="e">Args or event.</param>
    public delegate void ExecutedEventHandler(ICommand sender, ExecutedEventArgs e);
    /// <summary>
    /// Handler for <see cref="ICommand{T}.Executed"/> event.
    /// </summary>
    /// <param name="sender"><see cref="ICommand{T}"/> that raise the event.</param>
    /// <param name="e">Args or event.</param>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public delegate void ExecutedEventHandler<T>(ICommand<T> sender, ExecutedEventArgs<T> e);

    /// <summary>
    /// Args for <see cref="ICommand.Executed"/> event.
    /// </summary>
    public class ExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// Create new instance of <see cref="ExecutedEventArgs{T}"/>.
        /// </summary>
        /// <param name="executedAction">Executed action of finished excution, i.e., previous <see cref="CommandBase.Current"/>.</param>
        public ExecutedEventArgs(IAsyncAction executedAction) : this(executedAction, null) { }

        /// <summary>
        /// Create new instance of <see cref="ExecutedEventArgs{T}"/>.
        /// </summary>
        /// <param name="executedAction">Executed action of finished excution, i.e., previous <see cref="CommandBase.Current"/>.</param>
        /// <param name="exception">Error during the execution.</param>
        public ExecutedEventArgs(IAsyncAction executedAction, Exception exception)
        {
            this.ExecutedAction = executedAction ?? throw new ArgumentNullException(nameof(executedAction));
            this.Exception = exception;
            this.Handled = exception is null;
        }

        /// <summary>
        /// Error during the finished execution.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// <see cref="Exception"/> is handled by event handlers or not.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Executed action of finished excution, i.e., previous <see cref="CommandBase.Current"/>.
        /// </summary>
        public IAsyncAction ExecutedAction { get; }
    }

    /// <summary>
    /// Args for <see cref="ICommand{T}.Executed"/> event.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class ExecutedEventArgs<T> : ExecutedEventArgs
    {
        /// <summary>
        /// Create new instance of <see cref="ExecutedEventArgs{T}"/>.
        /// </summary>
        /// <param name="executedAction">Executed action of finished excution, i.e., previous <see cref="CommandBase.Current"/>.</param>
        /// <param name="parameter">Parameter of current execution.</param>
        public ExecutedEventArgs(IAsyncAction executedAction, T parameter) : base(executedAction)
        {
            this.Parameter = parameter;
        }

        /// <summary>
        /// Create new instance of <see cref="ExecutedEventArgs{T}"/>.
        /// </summary>
        /// <param name="executedAction">Executed action of finished excution, i.e., previous <see cref="CommandBase.Current"/>.</param>
        /// <param name="parameter">Parameter of current execution.</param>
        /// <param name="exception">Error during the execution.</param>
        public ExecutedEventArgs(IAsyncAction executedAction, T parameter, Exception exception)
            : base(executedAction, exception)
        {
            this.Parameter = parameter;
        }

        /// <summary>
        /// Parameter of finished execution.
        /// </summary>
        public T Parameter { get; }
    }
}
