namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Represent command without parameter.
    /// </summary>
    public interface ICommand : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Check whether the command can execute.
        /// </summary>
        /// <returns>Whether the command can execute or not</returns>
        bool CanExecute();

        /// <summary>
        /// Execute the <see cref="ICommand"/>.
        /// </summary>
        /// <returns>Whether execution started or not.</returns>
        bool Execute();

        /// <summary>
        /// Will be raised before execution.
        /// </summary>
        event ExecutingEventHandler Executing;
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        event ExecutedEventHandler Executed;
    }

    /// <summary>
    /// Represent command with parameter of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public interface ICommand<T> : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Check whether the command can execute.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether the command can execute or not</returns>
        bool CanExecute(T parameter);

        /// <summary>
        /// Execute the <see cref="ICommand{T}"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether execution started or not.</returns>
        bool Execute(T parameter);

        /// <summary>
        /// Will be raised before execution.
        /// </summary>
        event ExecutingEventHandler<T> Executing;
        /// <summary>
        /// Will be raised after execution.
        /// </summary>
        event ExecutedEventHandler<T> Executed;
    }
}