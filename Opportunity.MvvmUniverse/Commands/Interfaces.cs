namespace Opportunity.MvvmUniverse.Commands
{
    public interface IControllable
    {
        /// <summary>
        /// A tag associated with this object.
        /// </summary>
        object Tag { get; set; }

        bool IsEnabled { get; set; }
    }

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

    public interface IAsyncCommand : System.Windows.Input.ICommand
    {
        bool IsExecuting { get; }
    }

    public interface ICommandWithProgress<TProgress> : IAsyncCommand
    {
        TProgress Progress { get; }
        double NormalizedProgress { get; }
    }

    public interface IAsyncCommandWithProgress<TProgress> : ICommand, ICommandWithProgress<TProgress>
    {
        /// <summary>
        /// Will be raised when <see cref="ICommandWithProgress{TProgress}.Progress"/> changed during execution.
        /// </summary>
        event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }

    public interface IAsyncCommandWithProgress<T, TProgress> : ICommand<T>, ICommandWithProgress<TProgress>
    {

        /// <summary>
        /// Will be raised when <see cref="ICommandWithProgress{TProgress}.Progress"/> changed during execution.
        /// </summary>
        event ProgressChangedEventHandler<T, TProgress> ProgressChanged;
    }
}