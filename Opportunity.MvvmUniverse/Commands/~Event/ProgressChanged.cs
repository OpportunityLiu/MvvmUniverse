using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Opportunity.MvvmUniverse.Commands
{
    /// <summary>
    /// Event handler for <see cref="IAsyncCommandWithProgress{TProgress}.ProgressChanged"/>.
    /// </summary>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    /// <param name="sender">Sender of event.</param>
    /// <param name="e">Args of event.</param>
    public delegate void ProgressChangedEventHandler<TProgress>(IAsyncCommandWithProgress<TProgress> sender, ProgressChangedEventArgs<TProgress> e);
    /// <summary>
    /// Event handler for <see cref="IAsyncCommandWithProgress{T,TProgress}.ProgressChanged"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    /// <param name="sender">Sender of event.</param>
    /// <param name="e">Args of event.</param>
    public delegate void ProgressChangedEventHandler<T, TProgress>(IAsyncCommandWithProgress<T, TProgress> sender, ProgressChangedEventArgs<T, TProgress> e);

    /// <summary>
    /// Event args for <see cref="IAsyncCommandWithProgress{TProgress}.ProgressChanged"/>.
    /// </summary>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    public sealed class ProgressChangedEventArgs<TProgress> : EventArgs
    {
        /// <summary>
        /// Create factory for <see cref="ProgressChangedEventArgs{TProgress}"/>.
        /// </summary>
        /// <param name="progress">Initial value for <see cref="Progress"/>.</param>
        /// <returns>Factory for <see cref="ProgressChangedEventArgs{TProgress}"/>.</returns>
        public static Factory Create(TProgress progress)
            => new Factory(progress);

        /// <summary>
        /// Factory for <see cref="ProgressChangedEventArgs{TProgress}"/>, enables modification of <see cref="ProgressChangedEventArgs{TProgress}.Progress"/>.
        /// </summary>
        public readonly struct Factory
        {
            internal Factory(TProgress progress)
            {
                this.EventArgs = new ProgressChangedEventArgs<TProgress>(progress);
            }

            /// <summary>
            /// Event args of this <see cref="Factory"/>.
            /// </summary>
            public readonly ProgressChangedEventArgs<TProgress> EventArgs;

            /// <summary>
            /// Writeable property for <see cref="ProgressChangedEventArgs{TProgress}.Progress"/> of <see cref="EventArgs"/>.
            /// </summary>
            public TProgress Progress
            {
                get => this.EventArgs.progress;
                set => this.EventArgs.progress = value;
            }
        }

        private ProgressChangedEventArgs(TProgress progress)
        {
            this.progress = progress;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TProgress progress;
        /// <summary>
        /// Progress of current execution.
        /// </summary>
        public TProgress Progress => this.progress;
    }

    /// <summary>
    /// Event args for <see cref="IAsyncCommandWithProgress{T,TProgress}.ProgressChanged"/>.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    /// <typeparam name="TProgress">Type of progress data.</typeparam>
    public sealed class ProgressChangedEventArgs<T, TProgress> : EventArgs
    {
        /// <summary>
        /// Create factory for <see cref="ProgressChangedEventArgs{TProgress}"/>.
        /// </summary>
        /// <param name="progress">Initial value for <see cref="Progress"/>.</param>
        /// <param name="parameter">Parameter of current execution.</param>
        /// <returns>Factory for <see cref="ProgressChangedEventArgs{TProgress}"/>.</returns>
        public static Factory Create(T parameter, TProgress progress)
            => new Factory(parameter, progress);

        /// <summary>
        /// Factory for <see cref="ProgressChangedEventArgs{T,TProgress}"/>, enables modification of <see cref="ProgressChangedEventArgs{T,TProgress}.Progress"/>.
        /// </summary>
        public readonly struct Factory
        {
            internal Factory(T parameter, TProgress progress)
            {
                this.EventArgs = new ProgressChangedEventArgs<T, TProgress>(parameter, progress);
            }

            /// <summary>
            /// Event args of this <see cref="Factory"/>.
            /// </summary>
            public readonly ProgressChangedEventArgs<T, TProgress> EventArgs;

            /// <summary>
            /// Writeable property for <see cref="ProgressChangedEventArgs{T, TProgress}.Progress"/> of <see cref="EventArgs"/>.
            /// </summary>
            public TProgress Progress
            {
                get => this.EventArgs.progress;
                set => this.EventArgs.progress = value;
            }
        }

        private ProgressChangedEventArgs(T parameter, TProgress progress)
        {
            this.Parameter = parameter;
            this.progress = progress;
        }

        /// <summary>
        /// Parameter of current execution.
        /// </summary>
        public T Parameter { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TProgress progress;
        /// <summary>
        /// Progress of current execution.
        /// </summary>
        public TProgress Progress => this.progress;
    }
}
