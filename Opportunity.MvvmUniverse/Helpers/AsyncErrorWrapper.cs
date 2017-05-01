using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Helpers
{
    public sealed class AsyncErrorWrapper<T> : IAsyncOperation<T>
    {
        public AsyncErrorWrapper(Exception error)
        {
            this.error = error ?? throw new ArgumentNullException(nameof(error));
        }

        private Exception error;

        public AsyncOperationCompletedHandler<T> Completed
        {
            get => this.completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Error);
            }
        }


        private AsyncOperationCompletedHandler<T> completed;

        public Exception ErrorCode => error;

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel() { }

        public void Close() { }

        public T GetResults() => throw new AggregateException(error);
    }

    public sealed class AsyncErrorWrapper : IAsyncAction
    {
        public static AsyncErrorWrapper Create(Exception error)
        {
            return new AsyncErrorWrapper(error);
        }

        public static AsyncErrorWrapper<TResult> Create<TResult>(Exception error)
        {
            return new AsyncErrorWrapper<TResult>(error);
        }

        public AsyncErrorWrapper(Exception error)
        {
            this.error = error ?? throw new ArgumentNullException(nameof(error));
        }

        private Exception error;

        public AsyncActionCompletedHandler Completed
        {
            get => completed;
            set
            {
                this.completed = value;
                value?.Invoke(this, AsyncStatus.Error);
            }
        }

        private AsyncActionCompletedHandler completed;

        public Exception ErrorCode => error;

        public uint Id => uint.MaxValue;

        public AsyncStatus Status => AsyncStatus.Completed;

        public void Cancel() { }

        public void Close() { }

        public void GetResults() => throw new AggregateException(error);
    }
}
