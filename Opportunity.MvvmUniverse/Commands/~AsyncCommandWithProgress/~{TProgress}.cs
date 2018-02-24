using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class AsyncCommandWithProgress<TProgress> : AsyncCommand, IAsyncCommandWithProgress<TProgress>
    {
        protected AsyncCommandWithProgress(ProgressMapper<TProgress> progressMapper, AsyncPredicate canExecute)
            : base(canExecute)
        {
            this.ProgressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
        }

        protected ProgressMapper<TProgress> ProgressMapper { get; }

        public TProgress Progress { get; private set; }

        public double NormalizedProgress { get; private set; }

        private void setProgress(TProgress progress)
        {
            this.Progress = progress;
            this.NormalizedProgress = ProgressMapper(this, progress);
            OnPropertyChanged(nameof(Progress), nameof(NormalizedProgress));
        }

        protected override void OnFinished(Task execution)
        {
            try { base.OnFinished(execution); }
            finally { setProgress(default); }
        }

        /// <summary>
        /// Set value of <see cref="Progress"/> and <see cref="NormalizedProgress"/>,
        /// and raise <see cref="ProgressChanged"/>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnProgress(ProgressChangedEventArgs<TProgress> e)
        {
            setProgress(e.Progress);
            var p = this.ProgressChanged;
            if (p == null)
                return;
            DispatcherHelper.BeginInvoke(() => p.Invoke(this, e));
        }

        /// <summary>
        /// Will be raised when <see cref="Progress"/> changed during execution.
        /// </summary>
        public event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }
}
