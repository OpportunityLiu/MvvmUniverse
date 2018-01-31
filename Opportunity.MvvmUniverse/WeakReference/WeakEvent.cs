﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.WeakReference
{
    public sealed class WeakEvent<TDelegate>
        where TDelegate : class
    {
        //private readonly List<WeakDelegate<TDelegate>> list = new List<WeakDelegate<TDelegate>>();

        //public void AddHandler(TDelegate handler)
        //{
        //    if (handler == null)
        //        return;
        //    AddHandler(new WeakDelegate<TDelegate>(handler));
        //}

        //public void AddHandler(WeakDelegate<TDelegate> handler)
        //{
        //    if (handler == null)
        //        return;
        //    if (this.list.Contains(handler))
        //        return;
        //    this.list.Add(handler);

        //}

        //public void RemoveHandler(TDelegate handler)
        //{
        //    if (handler == null)
        //        return;
        //    RemoveHandler(new WeakDelegate<TDelegate>(handler));
        //}

        //public void RemoveHandler(WeakDelegate<TDelegate> handler)
        //{
        //    if (handler == null)
        //        return;
        //    this.list.Remove(handler);
        //}

        //public IList<WeakDelegate<TDelegate>> GetInvocationList() => this.list.ToArray();

        //public int InvocationListCount => this.list.Count;

        //public void Invoke(params object[] parameters)
        //{
        //    if (InvocationListCount == 0)
        //        return;
        //    RemoveUnavailableDelegates();
        //    foreach (var item in this.list)
        //    {
        //        if (item.IsAlive)
        //            item.DynamicInvoke(parameters);
        //    }
        //}

        //public void RemoveUnavailableDelegates()
        //{
        //    this.list.RemoveAll(d => !d.IsAlive);
        //}
    }
}
