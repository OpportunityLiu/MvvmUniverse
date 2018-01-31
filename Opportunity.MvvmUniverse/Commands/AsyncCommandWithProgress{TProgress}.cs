using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate double ProgressMapper<in TProgress>(TProgress progress);

    public abstract class AsyncCommandWithProgress<TProgress> : AsyncCommand, IProgressedCommand<TProgress>
    {
        protected AsyncCommandWithProgress(ProgressMapper<TProgress> progressMapper, AsyncCommandPredicate canExecute) : base(canExecute)
        {
            this.progressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
        }

        private readonly ProgressMapper<TProgress> progressMapper;
        protected ProgressMapper<TProgress> ProgressMapper => this.progressMapper;

        private TProgress progress;
        public TProgress Progress
        {
            get => this.progress;
            set => ForceSet(nameof(NormalizedProgress), ref this.progress, value);
        }

        public double NormalizedProgress => this.progressMapper(Progress);

        protected override void OnError(Exception error)
        {
            base.OnError(error);
            this.Progress = default;
        }

        protected override void OnFinished()
        {
            base.OnFinished();
            this.Progress = default;
        }
    }
}
