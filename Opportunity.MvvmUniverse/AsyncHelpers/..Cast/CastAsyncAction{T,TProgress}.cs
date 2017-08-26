using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    internal sealed class CastAsyncAction<T, TProgress> : IAsyncActionWithProgress<TProgress>
    {
        private readonly IAsyncOperationWithProgress<T, TProgress> operation;

        public CastAsyncAction(IAsyncOperationWithProgress<T, TProgress> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.operation.Completed = operationCompleted;
            this.operation.Progress = operationProgress;
        }

        private void operationCompleted(IAsyncOperationWithProgress<T, TProgress> asyncInfo, AsyncStatus asyncStatus)
        {
            this.completedHandler?.Invoke(this, asyncStatus);
        }

        private void operationProgress(IAsyncOperationWithProgress<T, TProgress> asyncInfo, TProgress progressInfo)
        {
            this.progressHandler?.Invoke(this, progressInfo);
        }

        public void GetResults() => this.operation.GetResults();

        private AsyncActionWithProgressCompletedHandler<TProgress> completedHandler;

        public AsyncActionWithProgressCompletedHandler<TProgress> Completed
        {
            get => this.completedHandler;
            set
            {
                if (this.completedHandler != null)
                    throw new InvalidOperationException("Completed has been set.");
                this.completedHandler = value ?? throw new ArgumentNullException(nameof(value));
                if (this.Status != AsyncStatus.Started)
                    operationCompleted(this.operation, this.Status);
            }
        }

        private AsyncActionProgressHandler<TProgress> progressHandler;

        public AsyncActionProgressHandler<TProgress> Progress
        {
            get => this.progressHandler;
            set
            {
                if (this.progressHandler != null)
                    throw new InvalidOperationException("Progress has been set.");
                this.progressHandler = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public void Cancel() => this.operation.Cancel();
        public void Close() => this.operation.Close();

        public Exception ErrorCode => this.operation.ErrorCode;

        public uint Id => this.operation.Id;

        public AsyncStatus Status => this.operation.Status;
    }
}
