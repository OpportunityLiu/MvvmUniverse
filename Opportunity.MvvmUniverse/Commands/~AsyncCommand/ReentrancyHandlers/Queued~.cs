using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// <see cref="IReentrancyHandler{T}"/> with a queue of parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    [DebuggerDisplay(@"{Name, nq}, QueueLength = {QueuedValues.Count}")]
    public class QueuedReentrancyHandler<T> : ReentrancyHandlerBase<T>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string Name
        {
            get
            {
                var name = ToString();
                var typeName = GetType().ToString();
                if (name == typeName && name.StartsWith("Opportunity.MvvmUniverse.Commands.ReentrancyHandlers."))
                {
                    name = name.Substring("Opportunity.MvvmUniverse.Commands.ReentrancyHandlers.".Length);
                    return name.Split(new[] { "ReentrancyHandler" }, StringSplitOptions.None).First();
                }
                return name;
            }
        }
        /// <summary>
        /// Items in queue.
        /// </summary>
        /// <remarks>
        /// When you changes this queue in sub classes directly,
        /// don't forget to call OnPropertyChanged to notify changes on <see cref="IsEmpty"/> and <see cref="PeekValue"/>.
        /// </remarks>
        protected ConcurrentQueue<T> QueuedValues { get; } = new ConcurrentQueue<T>();

        /// <summary>
        /// Get the item from the beginning of the queue, or default value of <typeparamref name="T"/>, if the queue is empty.
        /// </summary>
        public T PeekValue
        {
            get
            {
                if (QueuedValues.TryPeek(out var r))
                    return r;
                return default;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the queue is empty.
        /// </summary>
        public bool IsEmpty => QueuedValues.IsEmpty;

        /// <summary>
        /// Clear the queue.
        /// </summary>
        public void Empty()
        {
            var itemRemoved = false;
            while (QueuedValues.TryDequeue(out _))
            {
                itemRemoved = true;
            }
            if (itemRemoved)
            {
                OnPropertyChanged(ConstPropertyChangedEventArgs.PeekValue);
                OnPropertyChanged(ConstPropertyChangedEventArgs.IsEmpty);
            }
        }

        /// <summary>
        /// Enqueue <paramref name="value"/> to <see cref="QueuedValues"/>.
        /// </summary>
        /// <param name="value">The parameter of reentered execution.</param>
        /// <returns><see langword="false"/>.</returns>
        public override bool Enqueue(T value)
        {
            var oe = IsEmpty;
            QueuedValues.Enqueue(value);
            OnPropertyChanged(ConstPropertyChangedEventArgs.PeekValue);
            if (oe)
                OnPropertyChanged(ConstPropertyChangedEventArgs.IsEmpty);
            return false;
        }

        /// <summary>
        /// Try dequeue <paramref name="value"/> from <see cref="QueuedValues"/>
        /// </summary>
        /// <param name="value">Dequeued value.</param>
        /// <returns><see langword="true"/> if <see cref="QueuedValues"/> is not empty.</returns>
        public override bool TryDequeue(out T value)
        {
            var oe = IsEmpty;
            var r = QueuedValues.TryDequeue(out value);
            OnPropertyChanged(ConstPropertyChangedEventArgs.PeekValue);
            if (!oe && IsEmpty)
                OnPropertyChanged(ConstPropertyChangedEventArgs.IsEmpty);
            return r;
        }

        /// <summary>
        /// Empty <see cref="QueuedValues"/>.
        /// </summary>
        public override void Detach()
        {
            Empty();
            base.Detach();
        }
    }

}