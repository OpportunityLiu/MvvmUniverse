namespace Opportunity.MvvmUniverse.Commands
{
    public interface IControllableCommand
    {
        object Tag { get; set; }

        bool IsEnabled { get; set; }
    }

    public interface IAsyncCommand
    {
        bool IsExecuting { get; }
    }

    public interface IProgressedCommand<TProgress> : IAsyncCommand
    {
        TProgress Progress { get; }
        double NormalizedProgress { get; }
    }

    public interface ICommand : System.Windows.Input.ICommand
    {
        bool CanExecute();

        /// <summary>
        /// Execute the <see cref="ICommand"/>.
        /// </summary>
        /// <returns>Whether execution started or not.</returns>
        bool Execute();

        event CommandExecutingEventHandler Executing;
        event CommandExecutedEventHandler Executed;
    }

    public interface ICommand<T> : System.Windows.Input.ICommand
    {

        bool CanExecute(T parameter);

        /// <summary>
        /// Execute the <see cref="ICommand{T}"/>.
        /// </summary>
        /// <param name="parameter">parameter of execution</param>
        /// <returns>Whether execution started or not.</returns>
        bool Execute(T parameter);

        event CommandExecutingEventHandler<T> Executing;
        event CommandExecutedEventHandler<T> Executed;
    }
}