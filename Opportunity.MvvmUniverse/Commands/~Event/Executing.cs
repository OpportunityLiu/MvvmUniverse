using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Handler for <see cref="ICommand.Executing"/> event.
    /// </summary>
    /// <param name="sender"><see cref="ICommand"/> that raise the event.</param>
    /// <param name="e">Args or event.</param>
    public delegate void ExecutingEventHandler(ICommand sender, ExecutingEventArgs e);
    /// <summary>
    /// Handler for <see cref="ICommand{T}.Executing"/> event.
    /// </summary>
    /// <param name="sender"><see cref="ICommand{T}"/> that raise the event.</param>
    /// <param name="e">Args or event.</param>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public delegate void ExecutingEventHandler<T>(ICommand<T> sender, ExecutingEventArgs<T> e);

    /// <summary>
    /// Args for <see cref="ICommand.Executing"/> event.
    /// </summary>
    public class ExecutingEventArgs : EventArgs
    {
        /// <summary>
        /// Create new instance of <see cref="ExecutingEventArgs"/>.
        /// </summary>
        public ExecutingEventArgs() { }

        private bool canceled = false;
        /// <summary>
        /// Should current execution be canceled or not.
        /// </summary>
        /// <exception cref="InvalidOperationException">The event has been handled.</exception>
        /// <remarks>Only handlers that has access to the thread that the command is executing can set this property.</remarks>
        public bool Canceled
        {
            get => this.canceled;
            set
            {
                if (this.locked)
                    throw new InvalidOperationException("The event has been handled.");
                this.canceled = value;
            }
        }

        private bool locked = false;

        internal void Lock() => this.locked = true;
    }

    /// <summary>
    /// Args for <see cref="ICommand{T}.Executing"/> event.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class ExecutingEventArgs<T> : ExecutingEventArgs
    {
        /// <summary>
        /// Create new instance of <see cref="ExecutingEventArgs{T}"/>.
        /// </summary>
        /// <param name="parameter">Parameter of current execution.</param>
        public ExecutingEventArgs(T parameter)
        {
            Parameter = parameter;
        }

        /// <summary>
        /// Parameter of current execution.
        /// </summary>
        public T Parameter { get; }
    }
}
