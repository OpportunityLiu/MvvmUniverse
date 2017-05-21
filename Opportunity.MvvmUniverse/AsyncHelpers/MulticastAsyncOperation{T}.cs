using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public sealed class MulticastAsyncOperation<T> : IAsyncOperation<T>
    {
        private IAsyncOperation<T> action;

        public MulticastAsyncOperation(IAsyncOperation<T> operation)
        {
            this.action = operation ?? throw new ArgumentNullException(nameof(operation));
            this.action.Completed = this.action_Completed;
        }

        private void action_Completed(IAsyncOperation<T> sender, AsyncStatus e)
        {
            if (Disposed)
                return;
            foreach (var item in this.completed)
            {
                item(this, e);
            }
        }

        public bool Disposed => this.action == null;

        public AsyncOperationCompletedHandler<T> Completed
        {
            get => completed.FirstOrDefault();
            set => completed.Add(value);
        }

        private List<AsyncOperationCompletedHandler<T>> completed = new List<AsyncOperationCompletedHandler<T>>();

        public Exception ErrorCode => this.action.ErrorCode;

        public uint Id => this.action.Id;

        public AsyncStatus Status => this.action.Status;

        public void Cancel() => this.action.Cancel();

        public void Close()
        {
            if (Disposed)
                return;
            this.action.Close();
            this.action = null;
            this.completed = null;
        }

        public T GetResults() => this.action.GetResults();
    }
}
