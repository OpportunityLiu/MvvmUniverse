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
    /// Mapping <see cref="ICommandWithProgress{TProgress}.Progress"/> to <see cref="ICommandWithProgress{TProgress}.NormalizedProgress"/>.
    /// </summary>
    /// <typeparam name="TProgress">Type of original progress.</typeparam>
    /// <param name="command">Caller command.</param>
    /// <param name="progress">Original progress.</param>
    /// <returns>Mapped progress.</returns>
    public delegate double ProgressMapper<TProgress>(IAsyncCommandWithProgress<TProgress> command, TProgress progress);

    public interface IAsyncCommandWithProgress<TProgress> : ICommand, ICommandWithProgress<TProgress>
    {
        /// <summary>
        /// Will be raised when <see cref="ICommandWithProgress{TProgress}.Progress"/> changed during execution.
        /// </summary>
        event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }

    /// <summary>
    /// Mapping <see cref="ICommandWithProgress{TProgress}.Progress"/> to <see cref="ICommandWithProgress{TProgress}.NormalizedProgress"/>.
    /// </summary>
    /// <typeparam name="T">Type of <paramref name="parameter"/>.</typeparam>
    /// <typeparam name="TProgress">Type of original progress.</typeparam>
    /// <param name="command">Caller command.</param>
    /// <param name="parameter">Parameter of current execution.</param>
    /// <param name="progress">Original progress.</param>
    /// <returns>Mapped progress.</returns>
    public delegate double ProgressMapper<T, TProgress>(IAsyncCommandWithProgress<T, TProgress> command, T parameter, TProgress progress);

    public interface IAsyncCommandWithProgress<T, TProgress> : ICommand<T>, ICommandWithProgress<TProgress>
    {

        /// <summary>
        /// Will be raised when <see cref="ICommandWithProgress{TProgress}.Progress"/> changed during execution.
        /// </summary>
        event ProgressChangedEventHandler<T, TProgress> ProgressChanged;
    }
}