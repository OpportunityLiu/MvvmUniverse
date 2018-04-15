using Opportunity.Helpers.ObjectModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Opportunity.MvvmUniverse.Commands.ReentrancyHandlers
{
    /// <summary>
    /// <see cref="IReentrancyHandler{T}"/> with a single slot of parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    [DebuggerDisplay(@"{Name, nq}, HasValue = {HasValue}, QueuedValue = {QueuedValue}")]
    public abstract class SingleQueuedReentrancyHandler<T> : ReentrancyHandlerBase<T>
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Box<T> queuedValue;
        /// <summary>
        /// Has value in queue.
        /// </summary>
        public bool HasValue => this.queuedValue != null;
        /// <summary>
        /// Value in queue.
        /// </summary>
        public T QueuedValue => this.queuedValue is null ? default : this.queuedValue.Value;

        /// <summary>
        /// Store <paramref name="value"/> to <see cref="QueuedValue"/>.
        /// </summary>
        /// <param name="value">The parameter of reentered execution.</param>
        /// <returns><see langword="false"/>.</returns>
        public override bool Enqueue(T value)
        {
            this.queuedValue = Box.Create(value);
            OnPropertyChanged(EventArgsConst.QueuedValuePropertyChanged);
            OnPropertyChanged(EventArgsConst.HasValuePropertyChanged);
            return false;
        }

        /// <summary>
        /// Try restore <paramref name="value"/> from <see cref="QueuedValue"/>
        /// </summary>
        /// <param name="value">Dequeued value.</param>
        /// <returns><see langword="true"/> if <see cref="HasValue"/>.</returns>
        public override bool TryDequeue(out T value)
        {
            var v = Interlocked.Exchange(ref this.queuedValue, null);
            if (v is null)
            {
                value = default;
                return false;
            }
            else
            {
                value = v.Value;
                OnPropertyChanged(EventArgsConst.QueuedValuePropertyChanged);
                OnPropertyChanged(EventArgsConst.HasValuePropertyChanged);
                return true;
            }
        }
    }

}