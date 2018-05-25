// Copyright (c) 2008 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Opportunity.MvvmUniverse
{
    /// <summary>
    /// A class for managing a weak event.
    /// </summary>
    /// <typeparam name="TDelegate">Delegate type of event</typeparam>
    /// <typeparam name="TSender">Type of sender</typeparam>
    /// <typeparam name="TEventArgs">Type of event args</typeparam>
    public sealed class WeakEvent<TDelegate, TSender, TEventArgs>
        where TDelegate : Delegate
    {
        private struct EventEntry
        {
            public readonly object TargetMethodOrDelegate;
            public readonly WeakReference TargetReference;

            public EventEntry(MethodInfo targetMethod, object targetReference)
            {
                this.TargetMethodOrDelegate = targetMethod;
                this.TargetReference = new WeakReference(targetReference);

            }

            public EventEntry(Delegate targetDelegate)
            {
                this.TargetMethodOrDelegate = targetDelegate;
                this.TargetReference = null;
            }
        }

        private readonly List<EventEntry> eventEntries = new List<EventEntry>();

        static WeakEvent()
        {
            var invoke = typeof(TDelegate).GetMethod("Invoke");
            if (invoke == null)
                throw new ArgumentException("TDelegate must have \"Invoke\" method", nameof(TDelegate));

            if (invoke.ReturnType != typeof(void))
                throw new ArgumentException("The delegate return type must be void.", nameof(TDelegate));

            var parameters = invoke.GetParameters();
            if (parameters.Length != 2)
                throw new ArgumentException("TDelegate must be a delegate type taking 2 parameters", nameof(TDelegate));

            var senderParameter = parameters[0];
            if (senderParameter.ParameterType != typeof(TSender))
                throw new ArithmeticException("TSender doesn't match the type of TDelegate.");

            var argsParameter = parameters[1];
            if (argsParameter.ParameterType != typeof(TEventArgs))
                throw new ArithmeticException("TEventArgs doesn't match the type of TDelegate.");
        }

        /// <summary>
        /// Add handler to event.
        /// </summary>
        /// <param name="eventHandler">handler to add</param>
        public void Add(TDelegate eventHandler)
        {
            if (eventHandler is null)
                return;
            var l = eventHandler.GetInvocationList();
            if (this.eventEntries.Count + l.Length > this.eventEntries.Capacity)
                removeDeadEntries();
            foreach (var item in l)
            {
                add(item);
            }
        }

        private void add(Delegate d)
        {
            var method = d.GetMethodInfo();
            if (method.DeclaringType.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).FirstOrDefault() != null)
                throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
            var target = d.Target;
            if (target == null)
                this.eventEntries.Add(new EventEntry(d));
            else

                this.eventEntries.Add(new EventEntry(method, target));
        }

        private void removeDeadEntries()
        {
            this.eventEntries.RemoveAll(ee => ee.TargetReference != null && !ee.TargetReference.IsAlive);
        }

        /// <summary>
        /// Remove handler from event.
        /// </summary>
        /// <param name="eventHandler">handler to remove</param>
        public void Remove(TDelegate eventHandler)
        {
            if (eventHandler is null)
                return;
            foreach (var item in eventHandler.GetInvocationList())
            {
                remove(item);
            }
        }

        private void remove(Delegate d)
        {
            for (var i = this.eventEntries.Count - 1; i >= 0; i--)
            {
                var entry = this.eventEntries[i];
                if (entry.TargetReference != null)
                {
                    var target = entry.TargetReference.Target;
                    if (target == null)
                    {
                        this.eventEntries.RemoveAt(i);
                    }
                    else if (target == d.Target && ((MethodInfo)entry.TargetMethodOrDelegate) == d.GetMethodInfo())
                    {
                        this.eventEntries.RemoveAt(i);
                        break;
                    }
                }
                else
                {
                    if (d.Target == null && (Delegate)entry.TargetMethodOrDelegate == d)
                    {
                        this.eventEntries.RemoveAt(i);
                        break;
                    }
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
            if (this.eventEntries.Count == 0)
                return;
            var needsCleanup = false;
            object[] parameters = null;
            foreach (var item in this.eventEntries.ToList())
            {
                if (item.TargetReference != null)
                {
                    var target = item.TargetReference.Target;
                    if (target != null)
                    {
                        if (parameters == null)
                            parameters = new object[] { sender, e };
                        ((MethodInfo)item.TargetMethodOrDelegate).Invoke(target, parameters);
                    }
                    else
                    {
                        needsCleanup = true;
                    }
                }
                else
                {
                    if (parameters == null)
                        parameters = new object[] { sender, e };
                    ((Delegate)item.TargetMethodOrDelegate).DynamicInvoke(null, parameters);
                }
            }
            if (needsCleanup)
                removeDeadEntries();
        }
    }
}