using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncWrappers
{
    public sealed partial class AsyncWrapper : IAsyncAction
    {
        internal AsyncWrapper(AsyncStatus status, Exception error)
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

        public Exception ErrorCode { get; }

        public uint Id => unchecked((uint)GetHashCode());

        public AsyncStatus Status { get; }

        public void Cancel() { }

        public void Close()
        {
            this.completed = null;
        }

        public void GetResults()
        {
            if (this.ErrorCode != null)
                throw new AggregateException(this.ErrorCode);
        }
    }
}
