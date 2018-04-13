using System;

namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// Handler for async command reentrancy.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public interface IReentrancyHandler<T>
    {
        /// <summary>
        /// Will be called when reentrance happens.
        /// </summary>
        /// <param name="value">The parameter of reentered execution.</param>
        /// <returns><see langword="true"/> if current execution should be canceled, otherwise, <see langword="false"/>.</returns>
        bool Enqueue(T value);
        /// <summary>
        /// Should <see cref="ICommand.CanExecute()"/> returns <see langword="true"/> while <see cref="IAsyncCommand.IsExecuting"/>.
        /// </summary>
        bool AllowReenter { get; }
        /// <summary>
        /// Will be called when execution finished.
        /// </summary>
        /// <param name="value">Parameter of queued execution.</param>
        /// <returns>Should a queued execution starts or not.</returns>
        bool TryDequeue(out T value);
        /// <summary>
        /// Attach to an <see cref="IAsyncCommand"/>.
        /// </summary>
        /// <param name="command">Command to attach.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The instance has attached to an <see cref="IAsyncCommand"/>.</exception>
        void Attach(IAsyncCommand command);
        /// <summary>
        /// Detach from current <see cref="IAsyncCommand"/>.
        /// </summary>
        void Detach();
    }

}