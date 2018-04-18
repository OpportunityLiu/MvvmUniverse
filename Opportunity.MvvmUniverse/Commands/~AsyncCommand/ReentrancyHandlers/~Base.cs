using System;
using System.Diagnostics;
using System.Threading;

namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// Base class for implementation of <see cref="IReentrancyHandler{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public abstract class ReentrancyHandlerBase<T> : ObservableObject, IReentrancyHandler<T>
    {
        /// <summary>
        /// Returns <see langword="true"/> by default.
        /// </summary>
        public virtual bool AllowReenter => true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IAsyncCommand command;
        /// <summary>
        /// Currently attached <see cref="IAsyncCommand"/> of this instance.
        /// </summary>
        public IAsyncCommand AttachedCommand => this.command;


        /// <summary>
        /// Set <see cref="AttachedCommand"/> to <paramref name="command"/>.
        /// </summary>
        /// <param name="command">Command to attach.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The instance has attached to an <see cref="IAsyncCommand"/>.</exception>
        public virtual void Attach(IAsyncCommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            var i = Interlocked.CompareExchange(ref this.command, command, null);
            if (i != null)
                throw new InvalidOperationException("This instance of " + GetType() + " has attached to a IAsyncCommand.");
            OnPropertyChanged(ConstPropertyChangedEventArgs.AttachedCommand);
        }
        /// <summary>
        /// Set <see cref="AttachedCommand"/> to <see langword="null"/>.
        /// </summary>
        public virtual void Detach()
        {
            this.command = null;
            OnPropertyChanged(ConstPropertyChangedEventArgs.AttachedCommand);
        }

        /// <inheritdoc/>
        public abstract bool Enqueue(T value);
        /// <inheritdoc/>
        public abstract bool TryDequeue(out T value);
    }

}