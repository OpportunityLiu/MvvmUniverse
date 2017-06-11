using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Windows.UI
{
    public static class DispatcherExtension
    {
        public static DispatcherAwaiterSource Yield(this CoreDispatcher dispatcher, CoreDispatcherPriority priority)
        {
            return new DispatcherAwaiterSource(dispatcher, priority);
        }
        public static DispatcherAwaiterSource Yield(this CoreDispatcher dispatcher)
        {
            return Yield(dispatcher, CoreDispatcherPriority.Normal);
        }
        public static DispatcherAwaiterSource YieldIdle(this CoreDispatcher dispatcher)
        {
            return Yield(dispatcher, CoreDispatcherPriority.Idle);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DispatcherAwaiterSource
    {
        internal DispatcherAwaiterSource(CoreDispatcher dispatcher, CoreDispatcherPriority priority)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
            if (priority > CoreDispatcherPriority.High)
                throw new ArgumentOutOfRangeException(nameof(priority));
            if (priority < CoreDispatcherPriority.Idle)
                throw new ArgumentOutOfRangeException(nameof(priority));
            this.awaiter = new DispatcherAwaiter(dispatcher, priority);
        }

        private readonly DispatcherAwaiter awaiter;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DispatcherAwaiter GetAwaiter() => this.awaiter;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public struct DispatcherAwaiter : INotifyCompletion
        {
            private readonly CoreDispatcher dispatcher;
            private readonly CoreDispatcherPriority priority;

            internal DispatcherAwaiter(CoreDispatcher dispatcher, CoreDispatcherPriority priority)
            {
                this.dispatcher = dispatcher;
                this.priority = priority;
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public async void OnCompleted(Action continuation)
            {
                if (this.priority == CoreDispatcherPriority.Idle)
                    await this.dispatcher.RunIdleAsync(a => continuation());
                else
                    await this.dispatcher.RunAsync(this.priority, () => continuation());
            }

            // yielding is always required for DispatcherAwaiter, hence false
            [EditorBrowsable(EditorBrowsableState.Never)]
            public bool IsCompleted => false;

            // Nop. It exists purely because the compiler pattern demands it.
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void GetResult() { }
        }
    }
}
