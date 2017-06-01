using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    internal sealed class MulticastAsyncAction : IAsyncAction
    {
        private IAsyncAction action;

        public MulticastAsyncAction(IAsyncAction action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.action.Completed = this.action_Completed;
        }

        private void action_Completed(IAsyncAction sender, AsyncStatus e)
        {
            if (Disposed)
                return;
            foreach (var item in this.completed)
            {
                item(this, e);
            }
        }

        public bool Disposed => this.action == null;

        public AsyncActionCompletedHandler Completed
        {
            get => completed.FirstOrDefault();
            set => completed.Add(value);
        }

        private List<AsyncActionCompletedHandler> completed = new List<AsyncActionCompletedHandler>();

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
