using System;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Represents a command with progress.
    /// </summary>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    public interface ICommandWithProgress<TProgress> : IAsyncCommand
    {
        /// <summary>
        /// Progress data of current execution. Will return default value if <see cref="IAsyncCommand.IsExecuting"/> is <see langword="false"/>.
        /// </summary>
        TProgress Progress { get; }
        /// <summary>
        /// Normalized progress of current execution, for binding usage.
        /// </summary>
        double NormalizedProgress { get; }
    }

    /// <summary>
    /// Represents a command without parameter, with progress.
    /// </summary>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    public interface IAsyncCommandWithProgress<TProgress> : ICommand, ICommandWithProgress<TProgress>
    {
        /// <summary>
        /// Will be raised when <see cref="ICommandWithProgress{TProgress}.Progress"/> changed during execution.
        /// </summary>
        event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }

    /// <summary>
    /// Represents a command with parameter of <typeparamref name="T"/>, with progress.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    public interface IAsyncCommandWithProgress<T, TProgress> : ICommand<T>, ICommandWithProgress<TProgress>
    {

        /// <summary>
        /// Will be raised when <see cref="ICommandWithProgress{TProgress}.Progress"/> changed during execution.
        /// </summary>
        event ProgressChangedEventHandler<T, TProgress> ProgressChanged;
    }
}