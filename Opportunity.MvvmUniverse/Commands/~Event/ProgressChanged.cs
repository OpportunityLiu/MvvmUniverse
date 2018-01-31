using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    public delegate void ProgressChangedEventHandler<TProgress>(IAsyncCommandWithProgress<TProgress> sender, ProgressChangedEventArgs<TProgress> e);
    public delegate void ProgressChangedEventHandler<T, TProgress>(IAsyncCommandWithProgress<T, TProgress> sender, ProgressChangedEventArgs<T, TProgress> e);

    public static class ProgressChangedEventArgsFactory
    {
        public static Factory<TProgress> Create<TProgress>(TProgress progress)
            => new Factory<TProgress>(progress);
        public static Factory<T, TProgress> Create<T, TProgress>(T parameter, TProgress progress)
            => new Factory<T, TProgress>(parameter, progress);

        public struct Factory<TProgress>
        {
            public Factory(TProgress progress)
            {
                this.EventArgs = new ProgressChangedEventArgs<TProgress>(progress);
            }

            public readonly ProgressChangedEventArgs<TProgress> EventArgs;

            public TProgress Progress
            {
                get => this.EventArgs.Progress;
                set => this.EventArgs.Progress = value;
            }
        }

        public struct Factory<T, TProgress>
        {
            public Factory(T parameter, TProgress progress)
            {
                this.EventArgs = new ProgressChangedEventArgs<T, TProgress>(parameter, progress);
            }

            public readonly ProgressChangedEventArgs<T, TProgress> EventArgs;

            public TProgress Progress
            {
                get => this.EventArgs.Progress;
                set => this.EventArgs.Progress = value;
            }
        }
    }

    public class ProgressChangedEventArgs<TProgress> : EventArgs
    {
        public ProgressChangedEventArgs(TProgress progress)
        {
            this.Progress = progress;
        }

        public TProgress Progress { get; internal set; }
    }

    public class ProgressChangedEventArgs<T, TProgress> : ProgressChangedEventArgs<TProgress>
    {
        public ProgressChangedEventArgs(T parameter, TProgress progress)
           : base(progress)
        {
            this.Parameter = parameter;
        }

        public T Parameter { get; }
    }
}
