using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public sealed class MulticastAsyncAction<TProgress> : IAsyncActionWithProgress<TProgress>
    {
        private IAsyncActionWithProgress<TProgress> action;

        public MulticastAsyncAction(IAsyncActionWithProgress<TProgress> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.action.Completed = this.action_Completed;
            this.action.Progress = this.action_Progress;
        }

        private void action_Completed(IAsyncActionWithProgress<TProgress> sender, AsyncStatus e)
        {
            if (Disposed)
                return;
            foreach (var item in this.completed)
            {
                item(this, e);
            }
        }

        private void action_Progress(IAsyncActionWithProgress<TProgress> asyncInfo, TProgress progressInfo)
        {
            if (Disposed)
                return;
            foreach (var item in this.progress)
            {
                item(this, progressInfo);
            }
        }

        public bool Disposed => this.action == null;

        public AsyncActionWithProgressCompletedHandler<TProgress> Completed
        {
            get => completed.FirstOrDefault();
            set => completed.Add(value);
        }

        private List<AsyncActionWithProgressCompletedHandler<TProgress>> completed
            = new List<AsyncActionWithProgressCompletedHandler<TProgress>>();

        public AsyncActionProgressHandler<TProgress> Progress
        {
            get => progress.FirstOrDefault();
            set => progress.Add(value);
        }

        private List<AsyncActionProgressHandler<TProgress>> progress
            = new List<AsyncActionProgressHandler<TProgress>>();

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

        public void GetResults() => this.action.GetResults();
    }
}
