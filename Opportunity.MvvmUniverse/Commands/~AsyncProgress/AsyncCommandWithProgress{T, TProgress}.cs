using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Commands
{
    public abstract class AsyncCommandWithProgress<T, TProgress> : AsyncCommand<T>, IAsyncCommandWithProgress<T, TProgress>
    {
        protected AsyncCommandWithProgress(ProgressMapper<T, TProgress> progressMapper, AsyncPredicate<T> canExecute)
            : base(canExecute)
        {
            this.ProgressMapper = progressMapper ?? throw new ArgumentNullException(nameof(progressMapper));
        }

        protected ProgressMapper<T, TProgress> ProgressMapper { get; }

        public TProgress Progress { get; private set; }

        public double NormalizedProgress { get; private set; }

        private void setProgress(T parameter, TProgress progress)
        {
            this.Progress = progress;
            this.NormalizedProgress = ProgressMapper(this, parameter, progress);
            OnPropertyChanged(nameof(Progress), nameof(NormalizedProgress));
        }

        protected override void OnFinished(Task execution, T paramenter)
        {
            base.OnFinished(execution, paramenter);
            setProgress(paramenter, default);
        }

        /// <summary>
        /// Set value of <see cref="Progress"/> and <see cref="NormalizedProgress"/>,
        /// and raise <see cref="ProgressChanged"/>.
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnProgress(ProgressChangedEventArgs<T, TProgress> e)
        {
            setProgress(e.Parameter, e.Progress);
            var p = this.ProgressChanged;
            if (p == null)
                return;
            DispatcherHelper.BeginInvoke(() => p.Invoke(this, e));
        }

        public event ProgressChangedEventHandler<T, TProgress> ProgressChanged;
    }
}
