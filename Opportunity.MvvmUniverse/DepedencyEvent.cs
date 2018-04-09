using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse
{
    [DebuggerDisplay(@"\{{TokenValue}\}")]
    [StructLayout(LayoutKind.Explicit, Pack = sizeof(ulong), Size = sizeof(ulong))]
    internal readonly struct Token
    {
        public Token(ulong value)
        {
            this.TokenValue = value;
        }

        public Token(EventRegistrationToken eventRegistrationToken) : this()
        {
            this.EventRegistrationToken = eventRegistrationToken;
        }

        [FieldOffset(0)]
        public readonly ulong TokenValue;
        [FieldOffset(0)]
        public readonly EventRegistrationToken EventRegistrationToken;
    }

    /// <summary>
    /// A class for managing event associated with dispatcher.
    /// </summary>
    /// <typeparam name="TDelegate">Delegate type of event</typeparam>
    /// <typeparam name="TSender">Type of sender</typeparam>
    /// <typeparam name="TEventArgs">Type of event args</typeparam>
    [DebuggerDisplay(@"InvocationListLength = {InvocationListLength}")]
    public sealed class DepedencyEvent<TDelegate, TSender, TEventArgs>
        where TDelegate : class
    {
        [DebuggerDisplay(@"\{{Token.TokenValue} {Dispatcher} {Delegate}\}")]
        private readonly struct EventEntry
        {
            public readonly TDelegate Delegate;
            public readonly CoreDispatcher Dispatcher;
            public readonly Token Token;

            public EventEntry(TDelegate targetDelegate, CoreDispatcher dispatcher, long token)
            {
                this.Delegate = targetDelegate;
                this.Dispatcher = dispatcher;
                this.Token = new Token(unchecked((ulong)token));
            }
        }

        private long version = 1;
        private EventEntry[] eventEntries = Array.Empty<EventEntry>();
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
        public int InvocationListLength => this.eventEntries.Length;

        /// <summary>
        /// Add handler to event.
        /// </summary>
        /// <param name="eventHandler">Handler to add.</param>
        public EventRegistrationToken Add(TDelegate eventHandler)
        {
            if (eventHandler is null)
                return default;

            var dispatcher = Window.Current.Dispatcher;
            return add(eventHandler, dispatcher).Token.EventRegistrationToken;
        }

        private EventEntry add(TDelegate d, CoreDispatcher dispatcher)
        {
            while (true)
            {
                var oldV = this.eventEntries;
                var newV = new EventEntry[oldV.Length + 1];
                Array.Copy(oldV, newV, oldV.Length);
                newV[oldV.Length] = new EventEntry(d, dispatcher, Interlocked.Increment(ref this.version));
                var oldV2 = Interlocked.CompareExchange(ref this.eventEntries, newV, oldV);
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

            return remove(eventHandler).Token.EventRegistrationToken;
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
                var oldV = this.eventEntries;
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
                var oldV2 = Interlocked.CompareExchange(ref this.eventEntries, newV, oldV);
                if (oldV == oldV2)
                    return oldV[index];
            }
        }

        private EventEntry remove(EventRegistrationToken d)
        {
            while (true)
            {
                var oldV = this.eventEntries;
                var index = -1;
                for (var i = 0; i < oldV.Length; i++)
                {
                    if (d.Equals(oldV[i].Token.EventRegistrationToken))
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
                var oldV2 = Interlocked.CompareExchange(ref this.eventEntries, newV, oldV);
                if (oldV == oldV2)
                    return oldV[index];
            }
        }

        /// <summary>
        /// Raise event for handlers associated with current dispatcher or no dispatcher.
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">args of event</param>
        public void RaiseHasThreadAccessOnly(TSender sender, TEventArgs e)
        {
            var entries = (EventEntry[])this.eventEntries.Clone();
            if (entries.Length == 0)
                return;
            var defaultDispatcher = CoreApplication.MainView?.Dispatcher;
            var c = entries.Length;
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.Dispatcher is null || entry.Dispatcher == defaultDispatcher || entry.Delegate is null)
                    continue;
                if (entry.Dispatcher.HasThreadAccess)
                    this.raiser(entry.Delegate, sender, e);
                entries[i] = default;
                c--;
            }
            if (c == 0)
                return;
            if (defaultDispatcher is null || defaultDispatcher.HasThreadAccess)
            {
                foreach (var item in entries)
                {
                    if (item.Delegate is null)
                        continue;
                    this.raiser(item.Delegate, sender, e);
                }
            }
        }

        /// <summary>
        /// Raise event.
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">args of event</param>
        public void Raise(TSender sender, TEventArgs e)
        {
            var entries = (EventEntry[])this.eventEntries.Clone();
            if (entries.Length == 0)
                return;
            var defaultDispatcher = CoreApplication.MainView?.Dispatcher;
            var c = entries.Length;
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry.Dispatcher is null || entry.Dispatcher == defaultDispatcher || entry.Delegate is null)
                    continue;
                if (entry.Dispatcher.HasThreadAccess)
                    this.raiser(entry.Delegate, sender, e);
                else
                    entry.Dispatcher.Begin(() => this.raiser(entry.Delegate, sender, e));
                entries[i] = default;
                c--;
            }
            if (c == 0)
                return;
            if (defaultDispatcher is null || defaultDispatcher.HasThreadAccess)
            {
                foreach (var item in entries)
                {
                    if (item.Delegate is null)
                        continue;
                    this.raiser(item.Delegate, sender, e);
                }
            }
            else
                defaultDispatcher.Begin(() =>
                {
                    foreach (var item in entries)
                    {
                        if (item.Delegate is null)
                            continue;
                        this.raiser(item.Delegate, sender, e);
                    }
                });
        }
    }
}