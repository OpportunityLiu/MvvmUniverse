using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Helpers
{
    public sealed class AsyncCanceledWrapper<T> : IAsyncOperation<T>
    {
        public AsyncCanceledWrapper()
        {
        }

        public AsyncOperationCompletedHandler<T> Completed
        {
            get => this.completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Canceled);
            }
        }


        private AsyncOperationCompletedHandler<T> completed;

        public Exception ErrorCode => new OperationCanceledException();

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel() { }

        public void Close() { }

        public T GetResults() => throw new OperationCanceledException();
    }

    public sealed class AsyncCanceledWrapper : IAsyncAction
    {
        public static AsyncCanceledWrapper Create()
        {
            return new AsyncCanceledWrapper();
        }

        public static AsyncCanceledWrapper<TResult> Create<TResult>()
        {
            return new AsyncCanceledWrapper<TResult>();
        }

        public AsyncCanceledWrapper()
        {
        }

        public AsyncActionCompletedHandler Completed
        {
            get => completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Canceled);
            }
        }

        private AsyncActionCompletedHandler completed;

        public Exception ErrorCode => new OperationCanceledException();

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel() { }

        public void Close() { }

        public void GetResults() => throw new OperationCanceledException();
    }
}
