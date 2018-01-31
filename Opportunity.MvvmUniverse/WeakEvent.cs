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
    public sealed class WeakEvent<T> where T : class
    {
        struct EventEntry
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

        readonly List<EventEntry> eventEntries = new List<EventEntry>();

        static WeakEvent()
        {
            if (!typeof(T).GetTypeInfo().IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("T must be a delegate type");
            var invoke = typeof(T).GetMethod("Invoke");
            if (invoke.ReturnType != typeof(void))
                throw new ArgumentException("The delegate return type must be void.");
        }

        public void Add(T eh)
        {
            if (eh == null)
                return;

            var d = (Delegate)(object)eh;
            var l = d.GetInvocationList();
            if (this.eventEntries.Count + l.Length > this.eventEntries.Capacity)
                RemoveDeadEntries();
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

        void RemoveDeadEntries()
        {
            this.eventEntries.RemoveAll(ee => ee.TargetReference != null && !ee.TargetReference.IsAlive);
        }

        public void Remove(T eh)
        {
            if (eh == null)
                return;

            var d = (Delegate)(object)eh;
            foreach (var item in d.GetInvocationList())
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

        public void Raise(object sender, EventArgs e)
        {
            var needsCleanup = false;
            var parameters = new[] { sender, e };
            foreach (var item in this.eventEntries.ToArray())
            {
                if (item.TargetReference != null)
                {
                    var target = item.TargetReference.Target;
                    if (target != null)
                    {
                        ((MethodInfo)item.TargetMethodOrDelegate).Invoke(target, parameters);
                    }
                    else
                    {
                        needsCleanup = true;
                    }
                }
                else
                {
                    ((Delegate)item.TargetMethodOrDelegate).DynamicInvoke(null, parameters);
                }
            }
            if (needsCleanup)
                RemoveDeadEntries();
        }
    }
}