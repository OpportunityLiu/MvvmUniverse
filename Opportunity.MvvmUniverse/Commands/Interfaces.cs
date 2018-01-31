namespace Opportunity.MvvmUniverse.Commands
{
    public interface IControllable
    {
        object Tag { get; set; }

        bool IsEnabled { get; set; }
    }

    public interface ICommand : System.Windows.Input.ICommand
    {
        bool CanExecute();

        /// <summary>
        /// Execute the <see cref="ICommand"/>.
        /// </summary>
        /// <returns>Whether execution started or not.</returns>
        bool Execute();

        event ExecutingEventHandler Executing;
        event ExecutedEventHandler Executed;
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

        event ExecutingEventHandler<T> Executing;
        event ExecutedEventHandler<T> Executed;
    }

    public interface IAsyncCommand : System.Windows.Input.ICommand
    {
        bool IsExecuting { get; }
    }

    public interface ICommandWithProgress<TProgress>
    {
        TProgress Progress { get; }
        double NormalizedProgress { get; }
    }

    public interface IAsyncCommandWithProgress<TProgress> : IAsyncCommand, ICommand, ICommandWithProgress<TProgress>
    {
        event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }

    public interface IAsyncCommandWithProgress<T, TProgress> : IAsyncCommand, ICommand<T>, ICommandWithProgress<TProgress>
    {
        event ProgressChangedEventHandler<T, TProgress> ProgressChanged;
    }
}