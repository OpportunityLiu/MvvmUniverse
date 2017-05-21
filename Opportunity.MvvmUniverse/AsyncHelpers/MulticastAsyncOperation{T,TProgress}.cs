using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public sealed class MulticastAsyncOperation<T, TProgress> : IAsyncOperationWithProgress<T, TProgress>
    {
        private IAsyncOperationWithProgress<T, TProgress> action;

        public MulticastAsyncOperation(IAsyncOperationWithProgress<T, TProgress> operation)
        {
            this.action = operation ?? throw new ArgumentNullException(nameof(operation));
            this.action.Completed = this.action_Completed;
            this.action.Progress = this.action_Progress;
        }

        private void action_Completed(IAsyncOperationWithProgress<T, TProgress> sender, AsyncStatus e)
        {
            if (Disposed)
                return;
            foreach (var item in this.completed)
            {
                item(this, e);
            }
        }

        private void action_Progress(IAsyncOperationWithProgress<T, TProgress> asyncInfo, TProgress progressInfo)
        {
            if (Disposed)
                return;
            foreach (var item in this.progress)
            {
                item(this, progressInfo);
            }
        }

        public bool Disposed => this.action == null;

        public AsyncOperationWithProgressCompletedHandler<T, TProgress> Completed
        {
            get => completed.FirstOrDefault();
            set => completed.Add(value);
        }

        private List<AsyncOperationWithProgressCompletedHandler<T, TProgress>> completed
            = new List<AsyncOperationWithProgressCompletedHandler<T, TProgress>>();

        public AsyncOperationProgressHandler<T, TProgress> Progress
        {
            get => progress.FirstOrDefault();
            set => progress.Add(value);
        }

        private List<AsyncOperationProgressHandler<T, TProgress>> progress
            = new List<AsyncOperationProgressHandler<T, TProgress>>();

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
