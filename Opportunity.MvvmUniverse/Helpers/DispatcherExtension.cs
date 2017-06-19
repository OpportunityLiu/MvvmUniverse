using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Windows.UI.Core
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
    public struct DispatcherAwaiterSource
    {
        internal DispatcherAwaiterSource(CoreDispatcher dispatcher, CoreDispatcherPriority priority)
        {
            if (priority > CoreDispatcherPriority.High)
                priority = CoreDispatcherPriority.High;
            if (priority < CoreDispatcherPriority.Idle)
                priority = CoreDispatcherPriority.Idle;
            if (dispatcher == null)
                this.awaiter = new EmptyDispatcherAwaiter();
            else if (priority == CoreDispatcherPriority.Idle)
                this.awaiter = new IdleDispatcherAwaiter(dispatcher);
            else
                this.awaiter = new NormalDispatcherAwaiter(dispatcher, priority);
        }

        private readonly IDispatcherAwaiter awaiter;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IDispatcherAwaiter GetAwaiter() => this.awaiter;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDispatcherAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }

        void GetResult();
    }

    internal sealed class EmptyDispatcherAwaiter : IDispatcherAwaiter
    {
        public EmptyDispatcherAwaiter() { }

        public bool IsCompleted => true;

        public void GetResult() { }

        public void OnCompleted(Action continuation)
        {
            continuation();
        }
    }

    internal sealed class IdleDispatcherAwaiter : IDispatcherAwaiter
    {
        private readonly CoreDispatcher dispatcher;

        public IdleDispatcherAwaiter(CoreDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public bool IsCompleted => false;

        public void GetResult() { }

        public async void OnCompleted(Action continuation)
        {
            await this.dispatcher.RunIdleAsync(a => continuation());
        }
    }

    internal sealed class NormalDispatcherAwaiter : IDispatcherAwaiter
    {
        private readonly CoreDispatcher dispatcher;
        private readonly CoreDispatcherPriority priority;

        internal NormalDispatcherAwaiter(CoreDispatcher dispatcher, CoreDispatcherPriority priority)
        {
            this.dispatcher = dispatcher;
            this.priority = priority;
        }

        public bool IsCompleted => false;

        public void GetResult() { }

        public async void OnCompleted(Action continuation)
        {
            await this.dispatcher.RunAsync(this.priority, () => continuation());
        }
    }
}
