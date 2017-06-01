using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    internal sealed class CastAcyncOperation<TFrom, TTo, TProgress> : IAsyncOperationWithProgress<TTo, TProgress>
    {
        public CastAcyncOperation(IAsyncOperationWithProgress<TFrom, TProgress> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        private readonly IAsyncOperationWithProgress<TFrom, TProgress> operation;

        public TTo GetResults() => (TTo)(object)this.operation.GetResults();

        private AsyncOperationWithProgressCompletedHandler<TTo, TProgress> completed;

        private void operationCompletedHandler(IAsyncOperationWithProgress<TFrom, TProgress> asyncInfo, AsyncStatus asyncStatus)
        {
            this.completed(this, asyncStatus);
        }

        public AsyncOperationWithProgressCompletedHandler<TTo, TProgress> Completed
        {
            get => this.completed;
            set
            {
                this.completed = value ?? throw new ArgumentNullException(nameof(value));
                this.operation.Completed = this.operationCompletedHandler;
            }
        }

        private AsyncOperationProgressHandler<TTo, TProgress> progress;

        private void operationProgressHandler(IAsyncOperationWithProgress<TFrom, TProgress> asyncInfo, TProgress progressInfo)
        {
            this.progress(this, progressInfo);
        }

        public AsyncOperationProgressHandler<TTo, TProgress> Progress
        {
            get => this.progress;
            set
            {
                this.progress = value ?? throw new ArgumentNullException(nameof(value));
                this.operation.Progress = this.operationProgressHandler;
            }
        }

        public void Cancel() => this.operation.Cancel();

        public void Close() => this.operation.Close();

        public Exception ErrorCode => this.operation.ErrorCode;

        public uint Id => this.operation.Id;

        public AsyncStatus Status => this.operation.Status;
    }
}
