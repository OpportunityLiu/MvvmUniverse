using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncWrappers
{
    public sealed class AsyncWrapper<T> : IAsyncOperation<T>
    {
        internal AsyncWrapper(AsyncStatus status, T result, Exception error)
        {
            this.Status = status;
            this.result = result;
            this.ErrorCode = error;
        }

        private T result;

        public AsyncOperationCompletedHandler<T> Completed
        {
            get => this.completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, this.Status);
            }
        }

        private AsyncOperationCompletedHandler<T> completed;

        public Exception ErrorCode { get; }

        public uint Id => unchecked((uint)GetHashCode());

        public AsyncStatus Status { get; }

        public void Cancel() { }

        public void Close()
        {
            this.completed = null;
        }

        public T GetResults()
        {
            if (this.ErrorCode != null)
                throw new AggregateException(this.ErrorCode);
            return this.result;
        }
    }
}
