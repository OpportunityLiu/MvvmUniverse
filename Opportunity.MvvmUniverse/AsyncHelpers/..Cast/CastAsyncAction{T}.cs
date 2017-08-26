using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    internal sealed class CastAsyncAction<T> : IAsyncAction
    {
        private readonly IAsyncOperation<T> operation;

        public CastAsyncAction(IAsyncOperation<T> operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
            this.operation.Completed = operationCompleted;
        }

        private void operationCompleted(IAsyncOperation<T> asyncInfo, AsyncStatus asyncStatus)
        {
            this.completedHandler?.Invoke(this, asyncStatus);
        }

        public void GetResults() => this.operation.GetResults();

        private AsyncActionCompletedHandler completedHandler;

        public AsyncActionCompletedHandler Completed
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

        public void Cancel() => this.operation.Cancel();
        public void Close() => this.operation.Close();

        public Exception ErrorCode => this.operation.ErrorCode;

        public uint Id => this.operation.Id;

        public AsyncStatus Status => this.operation.Status;
    }
}
