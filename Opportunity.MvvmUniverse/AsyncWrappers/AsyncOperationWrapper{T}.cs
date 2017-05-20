using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncWrappers
{
    internal sealed class AsyncOperationWrapper<T> : IAsyncOperation<T>
    {
        internal AsyncOperationWrapper(AsyncStatus status, T result, Exception error)
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

        public Exception ErrorCode { get; private set; }

        public uint Id => unchecked((uint)GetHashCode());

        public AsyncStatus Status { get; }

        public void Cancel() { }

        public void Close()
        {
            this.completed = null;
            this.ErrorCode = null;
            this.result = default(T);
        }

        public T GetResults()
        {
            if (this.ErrorCode != null)
                throw new AggregateException(this.ErrorCode);
            return this.result;
        }
    }
}
