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

        /// <summary>
        /// Should current execution be canceled or not.
        /// </summary>
        public bool Canceled { get; set; }
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
