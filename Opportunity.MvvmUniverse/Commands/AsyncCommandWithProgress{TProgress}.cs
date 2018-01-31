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
            this.progressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
        }

        private readonly ProgressMapper<TProgress> progressMapper;
        protected ProgressMapper<TProgress> ProgressMapper => this.progressMapper;

        private TProgress progress;
        public TProgress Progress
        {
            get => this.progress;
            private set => ForceSet(nameof(NormalizedProgress), ref this.progress, value);
        }

        public double NormalizedProgress => this.progressMapper(Progress);

        protected override void OnFinished(ExecutedEventArgs e)
        {
            base.OnFinished(e);
            this.Progress = default;
        }

        protected virtual void OnProgress(ProgressChangedEventArgs<TProgress> e)
        {
            this.Progress = e.Progress;
            var p = this.ProgressChanged;
            if (p == null)
                return;
            DispatcherHelper.BeginInvoke(() => p.Invoke(this, e));
        }

        public event ProgressChangedEventHandler<TProgress> ProgressChanged;
    }
}
