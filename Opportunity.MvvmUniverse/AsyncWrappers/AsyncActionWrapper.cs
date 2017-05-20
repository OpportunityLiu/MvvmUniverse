using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncWrappers
{
    internal sealed class AsyncActionWrapper : IAsyncAction
    {
        internal AsyncActionWrapper(AsyncStatus status, Exception error)
        {
            this.Status = status;
            this.ErrorCode = error;
        }

        public AsyncActionCompletedHandler Completed
        {
            get => completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, this.Status);
            }
        }

        private AsyncActionCompletedHandler completed;

        public Exception ErrorCode { get; private set; }

        public uint Id => unchecked((uint)GetHashCode());

        public AsyncStatus Status { get; }

        public void Cancel() { }

        public void Close()
        {
            this.completed = null;
            this.ErrorCode = null;
        }

        public void GetResults()
        {
            if (this.ErrorCode != null)
                throw new AggregateException(this.ErrorCode);
        }
    }
}
