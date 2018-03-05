using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Opportunity.MvvmUniverse.Views
{
    public sealed class NavigationHandlerCollection : ObservableList<INavigationHandler>
    {
        internal static readonly Dictionary<INavigationHandler, Navigator> NavigationHandlerDic
            = new Dictionary<INavigationHandler, Navigator>();

        private readonly Navigator navigator;

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

        private static LockHelper GetLock() => new LockHelper(Navigator.NavigatorDictionary.Count > 1);

        internal NavigationHandlerCollection(Navigator navigator)
        {
            this.navigator = navigator;
        }

        protected override void ClearItems()
        {
            using (GetLock())
            {
                foreach (var item in this)
                {
                    item.OnRemove();
                    NavigationHandlerDic.Remove(item);
                }
            }
            base.ClearItems();
            this.navigator.UpdateAppViewBackButtonVisibility();
        }

        protected override void InsertItem(int index, INavigationHandler item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            using (GetLock())
            {
                NavigationHandlerDic.Add(item, this.navigator);
            }
            base.InsertItem(index, item);
            item.OnAdd(this.navigator);
            this.navigator.UpdateAppViewBackButtonVisibility();
        }

        protected override void SetItem(int index, INavigationHandler item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            using (GetLock())
            {
                var old = this[index];
                old.OnRemove();
                NavigationHandlerDic.Remove(old);
                NavigationHandlerDic.Add(item, this.navigator);
            }
            base.SetItem(index, item);
            item.OnAdd(this.navigator);
            this.navigator.UpdateAppViewBackButtonVisibility();
        }

        protected override void RemoveItem(int index)
        {
            using (GetLock())
            {
                var old = this[index];
                old.OnRemove();
                NavigationHandlerDic.Remove(old);
            }
            base.RemoveItem(index);
            this.navigator.UpdateAppViewBackButtonVisibility();
        }
    }
}
