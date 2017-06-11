using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;

namespace Windows.UI
{
    public static class DispatcherExtension
    {
        public static DispatcherAwaiterSource Yield(this CoreDispatcher dispatcher, CoreDispatcherPriority priority)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
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

        public static IAsyncAction RunAsync(this CoreDispatcher dispatcher, DispatchedHandler agileCallback)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, agileCallback);
        }

        private static async void beginCore(CoreDispatcher dispatcher, DispatchedHandler agileCallback, CoreDispatcherPriority priority)
        {
            await dispatcher.RunAsync(priority, agileCallback);
        }

        public static void Begin(this CoreDispatcher dispatcher, DispatchedHandler agileCallback, CoreDispatcherPriority priority)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
            if (agileCallback == null)
                throw new ArgumentNullException(nameof(agileCallback));
            if (priority == CoreDispatcherPriority.Idle)
                beginIdleCore(dispatcher, a => agileCallback());
            else
                beginCore(dispatcher, agileCallback, priority);
        }

        public static void Begin(this CoreDispatcher dispatcher, DispatchedHandler agileCallback)
        {
            Begin(dispatcher, agileCallback, CoreDispatcherPriority.Normal);
        }

        private static async void beginIdleCore(CoreDispatcher dispatcher, IdleDispatchedHandler agileCallback)
        {
            await dispatcher.RunIdleAsync(agileCallback);
        }

        public static void BeginIdle(this CoreDispatcher dispatcher, IdleDispatchedHandler agileCallback)
        {
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));
            beginIdleCore(dispatcher, agileCallback);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DispatcherAwaiterSource
    {
        internal DispatcherAwaiterSource(CoreDispatcher dispatcher, CoreDispatcherPriority priority)
        {
            if (priority > CoreDispatcherPriority.High)
                throw new ArgumentOutOfRangeException(nameof(priority));
            if (priority < CoreDispatcherPriority.Idle)
                throw new ArgumentOutOfRangeException(nameof(priority));
            this.awaiter = new DispatcherAwaiter(dispatcher, priority);
        }

        private readonly DispatcherAwaiter awaiter;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DispatcherAwaiter GetAwaiter() => this.awaiter;
    }

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
        public void OnCompleted(Action continuation)
        {
            if (this.dispatcher == null)
                onCompletedNull(continuation);
            else if (this.priority == CoreDispatcherPriority.Idle)
                onCompletedIdle(continuation);
            else
                onCompleted(continuation);
        }

        private void onCompletedNull(Action continuation)
        {
            continuation();
        }

        private async void onCompleted(Action continuation)
        {
            await this.dispatcher.RunAsync(this.priority, () => continuation());
        }

        private async void onCompletedIdle(Action continuation)
        {
            await this.dispatcher.RunIdleAsync(a => continuation());
        }

        // When dispatcher is null, OnCompleted shouldn't be called.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsCompleted => this.dispatcher == null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void GetResult() { }
    }
}
