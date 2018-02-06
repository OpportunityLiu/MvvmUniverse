namespace Opportunity.MvvmUniverse.Commands
{
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