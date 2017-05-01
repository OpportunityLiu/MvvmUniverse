﻿using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Helpers
{
    public sealed class AsyncWarpper<T> : IAsyncOperation<T>
    {
        public AsyncWarpper(T result)
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

        public void Cancel()
        {
        }

        public void Close()
        {
        }

        public T GetResults() => this.result;
    }

    public sealed class AsyncWarpper: IAsyncAction
    {
        public static AsyncWarpper Create()
        {
            return new AsyncWarpper();
        }

        public static AsyncWarpper<TResult> Create<TResult>()
        {
            return new AsyncWarpper<TResult>(default(TResult));
        }

        public static AsyncWarpper<TResult> Create<TResult>(TResult result)
        {
            return new AsyncWarpper<TResult>(result);
        }

        private AsyncWarpper() { }

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

        public void Cancel()
        {
        }

        public void Close()
        {
        }

        public void GetResults()
        {
        }
    }
}
