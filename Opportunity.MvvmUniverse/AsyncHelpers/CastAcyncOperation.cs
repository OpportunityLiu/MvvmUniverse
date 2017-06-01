using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    internal sealed class CastAcyncOperation<TFrom, TTo> : IAsyncOperation<TTo>
    {
        public CastAcyncOperation(IAsyncOperation<TFrom> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        private readonly IAsyncOperation<TFrom> operation;

        public TTo GetResults() => (TTo)(object)this.operation.GetResults();

        private AsyncOperationCompletedHandler<TTo> completed;

        private void operationCompletedHandler(IAsyncOperation<TFrom> asyncInfo, AsyncStatus asyncStatus)
        {
            this.completed(this, asyncStatus);
        }

        public AsyncOperationCompletedHandler<TTo> Completed
        {
            get => this.completed;
            set
            {
                this.completed = value ?? throw new ArgumentNullException(nameof(value));
                this.operation.Completed = this.operationCompletedHandler;
            }
        }

        public void Cancel() => this.operation.Cancel();

        public void Close() => this.operation.Close();

        public Exception ErrorCode => this.operation.ErrorCode;

        public uint Id => this.operation.Id;

        public AsyncStatus Status => this.operation.Status;
    }
}
