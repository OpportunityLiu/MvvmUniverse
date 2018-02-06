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

    public sealed class ProgressChangedEventArgs<TProgress> : EventArgs
    {
        public static Factory Create(TProgress progress)
            => new Factory(progress);

        public struct Factory
        {
            internal Factory(TProgress progress)
            {
                this.EventArgs = new ProgressChangedEventArgs<TProgress>(progress);
            }

            public readonly ProgressChangedEventArgs<TProgress> EventArgs;

            public ref TProgress Progress => ref this.EventArgs.progress;
        }

        public ProgressChangedEventArgs(TProgress progress)
        {
            this.progress = progress;
        }

        private TProgress progress;
        public TProgress Progress => this.progress;
    }

    public sealed class ProgressChangedEventArgs<T, TProgress> : EventArgs
    {
        public static Factory Create(T parameter, TProgress progress)
            => new Factory(parameter, progress);

        public struct Factory
        {
            internal Factory(T parameter, TProgress progress)
            {
                this.EventArgs = new ProgressChangedEventArgs<T, TProgress>(parameter, progress);
            }

            public readonly ProgressChangedEventArgs<T, TProgress> EventArgs;

            public ref TProgress Progress => ref this.EventArgs.progress;
        }

        public ProgressChangedEventArgs(T parameter, TProgress progress)
        {
            this.Parameter = parameter;
            this.progress = progress;
        }

        public T Parameter { get; }

        private TProgress progress;
        public TProgress Progress => this.progress;
    }
}
