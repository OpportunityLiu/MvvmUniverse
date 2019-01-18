using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse
{
    /// <summary>
    /// A class for managing event associated with dispatcher.
    /// </summary>
    /// <typeparam name="TDelegate">Delegate type of event</typeparam>
    /// <typeparam name="TSender">Type of sender</typeparam>
    /// <typeparam name="TEventArgs">Type of event args</typeparam>
    [DebuggerTypeProxy(typeof(DepedencyEvent<,,>.DebuggerProxy))]
    [DebuggerDisplay(@"InvocationListLength = {InvocationListLength}")]
    public sealed class DepedencyEvent<TDelegate, TSender, TEventArgs>
        where TDelegate : Delegate
    {
        [DebuggerDisplay(@"Token = {Token.Value}, Dispatcher = {DispatcherDisplay,nq}")]
        private readonly struct EventEntry
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private object DispatcherDisplay
            {
                get
                {
                    if (this.Dispatcher is null)
                        return "null";
                    if (!this.Dispatcher.TryGetTarget(out var target))
                        return "invalid";
                    return target;
                }
            }
            public readonly TDelegate Delegate;
            public readonly WeakReference<CoreDispatcher> Dispatcher;
            public readonly EventRegistrationToken Token;

            public unsafe EventEntry(TDelegate targetDelegate, CoreDispatcher dispatcher, long token)
            {
                this.Delegate = targetDelegate;
                if (dispatcher != null)
                    this.Dispatcher = new WeakReference<CoreDispatcher>(dispatcher);
                else
                    this.Dispatcher = default;
                this.Token = *(EventRegistrationToken*)&token;
            }
        }

        private class DebuggerProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly DepedencyEvent<TDelegate, TSender, TEventArgs> e;

            public DebuggerProxy(DepedencyEvent<TDelegate, TSender, TEventArgs> e)
            {
                this.e = e;
            }

            public int InvocationListLength => this.e.invocationList.Length;
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            protected EventEntry[] InvocationListValues => this.e.invocationList;
            protected Action<TDelegate, TSender, TEventArgs> Raiser => this.e.raiser;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private long version;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventEntry[] invocationList = Array.Empty<EventEntry>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Action<TDelegate, TSender, TEventArgs> raiser;

        /// <summary>
        /// Create new instance of <see cref="DepedencyEvent{TDelegate, TSender, TEventArgs}"/>
        /// </summary>
        /// <param name="raiser">Delegate to raise the event.</param>
        public DepedencyEvent(Action<TDelegate, TSender, TEventArgs> raiser)
        {
            this.raiser = raiser ?? throw new ArgumentNullException(nameof(raiser));
        }

        /// <summary>
        /// Length of invocation list.
        /// </summary>
        public int InvocationListLength => this.invocationList.Length;

        /// <summary>
        /// Add handler to event.
        /// </summary>
        /// <param name="eventHandler">Handler to add.</param>
        /// <returns><see cref=" System.Runtime.InteropServices.WindowsRuntime.EventRegistrationToken"/> or the registered handler.</returns>
        public EventRegistrationToken Add(TDelegate eventHandler)
        {
            if (eventHandler is null)
                return default;

            var dispatcher = Window.Current?.Dispatcher;
            return add(eventHandler, dispatcher).Token;
        }

        private EventEntry add(TDelegate d, CoreDispatcher dispatcher)
        {
            while (true)
            {
                var oldV = this.invocationList;
                var newV = new EventEntry[oldV.Length + 1];
                Array.Copy(oldV, newV, oldV.Length);
                newV[oldV.Length] = new EventEntry(d, dispatcher, Interlocked.Increment(ref this.version));
                var oldV2 = Interlocked.CompareExchange(ref this.invocationList, newV, oldV);
                if (oldV == oldV2)
                    return newV[oldV.Length];
            }
        }

        /// <summary>
        /// Remove handler from event.
        /// </summary>
        /// <param name="eventHandler">Handler to remove.</param>
        public EventRegistrationToken Remove(TDelegate eventHandler)
        {
            if (eventHandler == null)
                return default;

            return remove(eventHandler).Token;
        }

        /// <summary>
        /// Remove handler from event.
        /// </summary>
        /// <param name="token">Token of handler to remove.</param>
        public TDelegate Remove(EventRegistrationToken token)
        {
            if (token == default)
                return default;

            return remove(token).Delegate;
        }

        private EventEntry remove(TDelegate d)
        {
            while (true)
            {
                var oldV = this.invocationList;
                var index = -1;
                for (var i = 0; i < oldV.Length; i++)
                {
                    if (d.Equals(oldV[i].Delegate))
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                    return default;
                var newV = new EventEntry[oldV.Length - 1];
                Array.Copy(oldV, newV, index);
                Array.Copy(oldV, index + 1, newV, index, oldV.Length - index - 1);
                var oldV2 = Interlocked.CompareExchange(ref this.invocationList, newV, oldV);
                if (oldV == oldV2)
                    return oldV[index];
            }
        }

        private EventEntry remove(EventRegistrationToken d)
        {
            while (true)
            {
                var oldV = this.invocationList;
                var index = -1;
                for (var i = 0; i < oldV.Length; i++)
                {
                    if (d == oldV[i].Token)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                    return default;
                var newV = new EventEntry[oldV.Length - 1];
                Array.Copy(oldV, newV, index);
                Array.Copy(oldV, index + 1, newV, index, oldV.Length - index - 1);
                var oldV2 = Interlocked.CompareExchange(ref this.invocationList, newV, oldV);
                if (oldV == oldV2)
                    return oldV[index];
            }
        }

        /// <summary>
        /// Clear invocation list.
        /// </summary>
        public void Clear() => this.invocationList = Array.Empty<EventEntry>();

        private Task raise(TSender sender, TEventArgs e)
        {
            var entries = this.invocationList;
            if (entries.Length == 0)
                return Task.CompletedTask;

            var completionSource = new TaskCompletionSource<int>();
            var completedCount = 0;

            var defaultDispatcher = DispatcherHelper.Default;

            void raiseOne(EventEntry entry)
            {
                try
                {
                    this.raiser(entry.Delegate, sender, e);
                }
                catch (Exception ex)
                {
                    completionSource.TrySetException(ex);
                    return;
                }
                Interlocked.Increment(ref completedCount);
                if (completedCount == entries.Length)
                    completionSource.TrySetResult(0);
            }

            void removeOne(EventEntry entry)
            {
                remove(entry.Token);
                Interlocked.Increment(ref completedCount);
                if (completedCount == entries.Length)
                    completionSource.TrySetResult(0);
            }

            try
            {
                if (defaultDispatcher is null || defaultDispatcher.HasThreadAccess)
                {
                    foreach (var entry in entries)
                    {
                        Debug.Assert(entry.Delegate != null);
                        Debug.Assert(entry.Token != default);
                        if (entry.Dispatcher is null)
                        {
                            raiseOne(entry);
                        }
                        else if (!entry.Dispatcher.TryGetTarget(out var dispatcher))
                        {
                            removeOne(entry);
                        }
                        else if (dispatcher == defaultDispatcher || dispatcher is null)
                        {
                            raiseOne(entry);
                        }
                        else
                        {
                            dispatcher.Begin(() => raiseOne(entry));
                        }
                    }
                }
                else
                {
                    var c = entries.Length;
                    foreach (var entry in entries)
                    {
                        Debug.Assert(entry.Delegate != null);
                        Debug.Assert(entry.Token != default);
                        if (entry.Dispatcher is null)
                            continue;
                        else if (!entry.Dispatcher.TryGetTarget(out var dispatcher))
                        {
                            removeOne(entry);
                            c--;
                        }
                        else if (dispatcher == defaultDispatcher || dispatcher is null)
                            continue;
                        else if (dispatcher.HasThreadAccess)
                        {
                            raiseOne(entry);
                            c--;
                        }
                        else
                        {
                            dispatcher.Begin(() => raiseOne(entry));
                            c--;
                        }
                    }
                    if (c != 0)
                        defaultDispatcher.Begin(() =>
                        {
                            foreach (var entry in entries)
                            {
                                if (entry.Dispatcher is null)
                                    raiseOne(entry);
                                else if (!entry.Dispatcher.TryGetTarget(out var dispatcher))
                                    continue;
                                else if (dispatcher == defaultDispatcher || dispatcher is null)
                                    raiseOne(entry);
                                else
                                    continue;
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                completionSource.SetException(ex);
            }

            return completionSource.Task;
        }

        /// <summary>
        /// Raise event.
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">args of event</param>
        public IAsyncAction RaiseAsync(TSender sender, TEventArgs e)
            => raise(sender, e).AsAsyncAction();
    }
}