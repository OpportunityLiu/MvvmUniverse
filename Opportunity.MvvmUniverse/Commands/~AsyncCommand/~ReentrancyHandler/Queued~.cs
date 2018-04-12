using System.Collections.Concurrent;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// <see cref="IReentrancyHandler{T}"/> with a queue of parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public class QueuedReentrancyHandler<T> : ReentrancyHandlerBase<T>
    {
        /// <summary>
        /// Items in queue.
        /// </summary>
        protected ConcurrentQueue<T> QueuedValues { get; } = new ConcurrentQueue<T>();

        /// <summary>
        /// Enqueue <paramref name="value"/> to <see cref="QueuedValues"/>.
        /// </summary>
        /// <param name="value">The parameter of reentered execution.</param>
        /// <returns><see langword="false"/>.</returns>
        public override bool Enqueue(T value)
        {
            QueuedValues.Enqueue(value);
            return false;
        }
        /// <summary>
        /// Try dequeue <paramref name="value"/> from <see cref="QueuedValues"/>
        /// </summary>
        /// <param name="value">Dequeued value.</param>
        /// <returns><see langword="true"/> if <see cref="QueuedValues"/> is not empty.</returns>
        public override bool TryDequeue(out T value) => QueuedValues.TryDequeue(out value);

        /// <summary>
        /// Empty <see cref="QueuedValues"/>.
        /// </summary>
        public override void Detach()
        {
            while (QueuedValues.TryDequeue(out _)) { }
            base.Detach();
        }
    }

}