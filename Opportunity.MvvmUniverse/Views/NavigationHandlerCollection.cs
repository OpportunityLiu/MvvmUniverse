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
                    NavigationHandlerDic.Remove(item);
                }
            }
            base.ClearItems();
            this.navigator.UpdateAppViewBackButtonVisibility();
        }

        protected override void InsertItems(int index, IReadOnlyList<INavigationHandler> items)
        {
            using (GetLock())
            {
                foreach (var item in items)
                {
                    NavigationHandlerDic.Add(item, this.navigator);
                }
            }
            base.InsertItems(index, items);
            this.navigator.UpdateAppViewBackButtonVisibility();
        }

        protected override void SetItems(int index, IReadOnlyList<INavigationHandler> items)
        {
            using (GetLock())
            {
                var offset = 0;
                foreach (var item in items)
                {
                    NavigationHandlerDic.Remove(this[index + offset]);
                    NavigationHandlerDic.Add(item, this.navigator);
                    offset++;
                }
            }
            base.SetItems(index, items);
            this.navigator.UpdateAppViewBackButtonVisibility();
        }

        protected override void RemoveItems(int index, int count)
        {
            using (GetLock())
            {
                for (var i = 0; i < count; i++)
                {
                    NavigationHandlerDic.Remove(this[index + i]);
                }
            }
            base.RemoveItems(index, count);
            this.navigator.UpdateAppViewBackButtonVisibility();
        }
    }
}
