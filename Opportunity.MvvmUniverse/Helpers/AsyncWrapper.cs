using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Helpers
{
    public sealed class AsyncWrapper<T> : IAsyncOperation<T>
    {
        public AsyncWrapper(T result)
        {
            this.result = result;
        }

        private T result;

        public AsyncOperationCompletedHandler<T> Completed
        {
            get => this.completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Completed);
            }
        }


        private AsyncOperationCompletedHandler<T> completed;

        public Exception ErrorCode => null;

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel() { }

        public void Close() { }

        public T GetResults() => this.result;
    }

    public sealed class AsyncWrapper : IAsyncAction
    {
        public static AsyncWrapper Create()
        {
            return new AsyncWrapper();
        }

        public static AsyncWrapper<TResult> Create<TResult>()
        {
            return new AsyncWrapper<TResult>(default(TResult));
        }

        public static AsyncWrapper<TResult> Create<TResult>(TResult result)
        {
            return new AsyncWrapper<TResult>(result);
        }

        public AsyncWrapper() { }

        public AsyncActionCompletedHandler Completed
        {
            get => completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Completed);
            }
        }

        private AsyncActionCompletedHandler completed;

        public Exception ErrorCode => null;

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel() { }

        public void Close() { }

        public void GetResults() { }
    }
}
