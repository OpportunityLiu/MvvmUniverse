using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate double ProgressMapper<in TProgress>(TProgress progress);

    public abstract class AsyncCommandWithProgress<TProgress> : AsyncCommand, IAsyncCommandWithProgress<TProgress>
    {
        protected AsyncCommandWithProgress(ProgressMapper<TProgress> progressMapper, AsyncPredicate canExecute) : base(canExecute)
        {
            this.ProgressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
        }

        public ProgressMapper<TProgress> ProgressMapper { get; }

        private TProgress progress;
        public TProgress Progress
        {
            get => this.progress;
            private set => ForceSet(nameof(NormalizedProgress), ref this.progress, value);
        }

        public double NormalizedProgress => this.ProgressMapper(Progress);

        protected override void OnFinished(ExecutedEventArgs e)
        {
            base.OnFinished(e);
            this.Progress = default;
        }

        /// <summary>
        /// Set value of <see cref="Progress"/> and <see cref="NormalizedProgress"/>,
        /// and raise <see cref="ProgressChanged"/>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnProgress(ProgressChangedEventArgs<TProgress> e)
        {
            this.Progress = e.Progress;
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
