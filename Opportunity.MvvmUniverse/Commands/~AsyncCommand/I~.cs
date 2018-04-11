using System;
using System.Collections.Concurrent;
using System.Threading;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Represents a command with async executor.
    /// </summary>
    public interface IAsyncCommand : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Indicates whether the command is executing. When <see langword="true"/>, <see cref="System.Windows.Input.ICommand.CanExecute(object)"/> will returns <see langword="false"/>.
        /// </summary>
        bool IsExecuting { get; }
    }

    public interface IReentrancyHandler<T>
    {
        void Enqueue(T value, IAsyncAction current);
        bool AllowReenter { get; }
        bool TryDequeue(out T value);
    }

    public sealed class DisallowedReentrancyHandler<T> : IReentrancyHandler<T>
    {
        public bool AllowReenter => false;

        public void Enqueue(T value, IAsyncAction current) { }
        public bool TryDequeue(out T value)
        {
            value = default;
            return false;
        }
    }

    public sealed class IgnoredReentrancyHandler<T> : IReentrancyHandler<T>
    {
        public bool AllowReenter => true;

        public void Enqueue(T value, IAsyncAction current) { }
        public bool TryDequeue(out T value)
        {
            value = default;
            return false;
        }
    }

    public class QueuedReentrancyHandler<T> : IReentrancyHandler<T>
    {
        public virtual bool AllowReenter => true;

        /// <summary>
        /// Items in queue.
        /// </summary>
        protected ConcurrentQueue<T> QueuedValues { get; } = new ConcurrentQueue<T>();

        public virtual void Enqueue(T value, IAsyncAction current) => QueuedValues.Enqueue(value);

        public virtual bool TryDequeue(out T value) => QueuedValues.TryDequeue(out value);
    }

    public abstract class SingleQueuedReentrancyHandler<T> : IReentrancyHandler<T>
    {
        public virtual bool AllowReenter => true;

        private Box<T> queuedValue;
        /// <summary>
        /// Has value in queue.
        /// </summary>
        public bool HasValue => this.queuedValue != null;
        /// <summary>
        /// Value in queue.
        /// </summary>
        public T QueuedValue => this.queuedValue is null ? default : this.queuedValue.Value;

        public virtual void Enqueue(T value, IAsyncAction current)
        {
            this.queuedValue = Box.Create(value);
        }

        public virtual bool TryDequeue(out T value)
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
                return true;
            }
        }
    }

    public sealed class RestartReentrancyHandler<T> : SingleQueuedReentrancyHandler<T>
    {
        public override void Enqueue(T value, IAsyncAction current)
        {
            base.Enqueue(value, current);
            current.Cancel();
        }
    }

    public sealed class FirstQueuedReentrancyHandler<T> : SingleQueuedReentrancyHandler<T>
    {
        public override void Enqueue(T value, IAsyncAction current)
        {
            if (!HasValue)
                base.Enqueue(value, current);
        }
    }

    public sealed class LastQueuedReentrancyHandler<T> : SingleQueuedReentrancyHandler<T>
    {
    }

}