﻿using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Collection of <see cref="INavigationHandler"/>.
    /// </summary>
    internal sealed class NavigationHandlerCollection : ObservableList<INavigationHandler>
    {
        internal static readonly Dictionary<INavigationHandler, Navigator> NavigationHandlerDic
            = new Dictionary<INavigationHandler, Navigator>();

        private struct LockHelper : IDisposable
        {
            private bool locked;

            public LockHelper(bool needLock)
            {
                this = new LockHelper();
                if (needLock)
                    Monitor.Enter(NavigationHandlerDic, ref this.locked);
            }

            public void Dispose()
            {
                if (this.locked)
                    Monitor.Exit(NavigationHandlerDic);
                this.locked = false;
            }
        }

        private static LockHelper GetLock() => new LockHelper(Navigator.Count > 1);

        private Navigator navigator;

        internal NavigationHandlerCollection(Navigator navigator)
        {
            this.navigator = navigator;
        }

        protected override void ClearItems()
        {
            CheckAvailable();
            try
            {
                using (GetLock())
                {
                    while (this.Count != 0)
                    {
                        var item = this[this.Count - 1];
                        item.OnRemove();
                        NavigationHandlerDic.Remove(item);
                        base.RemoveItem(this.Count - 1);
                    }
                }
            }
            finally
            {
                this.navigator.UpdateProperties();
            }
        }

        protected override void InsertItem(int index, INavigationHandler item)
        {
            CheckAvailable();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            item.OnAdd(this.navigator);
            using (GetLock())
            {
                NavigationHandlerDic.Add(item, this.navigator);
            }
            base.InsertItem(index, item);
            this.navigator.UpdateProperties();
        }

        protected override void SetItem(int index, INavigationHandler item)
        {
            CheckAvailable();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            var old = this[index];
            item.OnAdd(this.navigator);
            try
            {
                old.OnRemove();
            }
            catch
            {
                item.OnRemove();
                throw;
            }
            using (GetLock())
            {
                NavigationHandlerDic.Remove(old);
                NavigationHandlerDic.Add(item, this.navigator);
            }
            base.SetItem(index, item);
            this.navigator.UpdateProperties();
        }

        protected override void RemoveItem(int index)
        {
            CheckAvailable();
            var old = this[index];
            old.OnRemove();
            using (GetLock())
            {
                NavigationHandlerDic.Remove(old);
            }
            base.RemoveItem(index);
            this.navigator.UpdateProperties();
        }

        internal void Destory()
        {
            if (this.navigator != null)
            {
                Clear();
                this.navigator = null;
            }
        }

        private void CheckAvailable()
        {
            if (this.navigator is null)
                throw new InvalidOperationException("The navigator of this collection has been destoryed.");
        }
    }
}
