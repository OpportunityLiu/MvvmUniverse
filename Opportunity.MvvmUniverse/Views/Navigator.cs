using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Specialized;
using Opportunity.MvvmUniverse.Helpers;

namespace Opportunity.MvvmUniverse.Views
{
    public sealed class Navigator : ObservableObject
    {
        private static readonly Dictionary<int, Navigator> dic = new Dictionary<int, Navigator>();

        public static Navigator CreateOrGetForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (dic.TryGetValue(id, out var r))
                return r;
            return dic[id] = new Navigator();
        }

        public static Navigator GetForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (dic.TryGetValue(id, out var r))
                return r;
            return null;
        }

        public static bool DestoryForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (!dic.TryGetValue(id, out var r))
                return false;
            dic.Remove(id);
            r.destory();
            return true;
        }

        public ObservableCollection<INavigationHandler> Handlers { get; private set; } = new ObservableCollection<INavigationHandler>();

        private SystemNavigationManager manager = SystemNavigationManager.GetForCurrentView();

        private Navigator()
        {
            this.manager.BackRequested += this.manager_BackRequested;
            this.Handlers.CollectionChanged += this.handlers_CollectionChanged;
        }

        public void UpdateAppViewBackButtonVisibility()
        {
            var ov = AppViewBackButtonVisibility;
            var nv = appViewBackButtonVisibilityOverride ?? (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            if (ov != nv)
            {
                AppViewBackButtonVisibility = nv;
                RaisePropertyChanged(nameof(AppViewBackButtonVisibility));
            }
        }

        public bool CanGoBack()
        {
            var canGoBack = false;
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                if (Handlers[i].CanGoBack())
                {
                    canGoBack = true;
                    break;
                }
            }
            return canGoBack;
        }

        public bool GoBack()
        {
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                var h = Handlers[i];
                if (h.CanGoBack())
                {
                    h.GoBack();
                    return true;
                }
            }
            return false;
        }

        private void handlers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.Cast<INavigationHandler>())
                {
                    item.Parent = null;
                }
            }
            foreach (var item in Handlers)
            {
                item.Parent = this;
            }
            UpdateAppViewBackButtonVisibility();
        }

        public AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get => manager.AppViewBackButtonVisibility;
            private set => manager.AppViewBackButtonVisibility = value;
        }

        private AppViewBackButtonVisibility? appViewBackButtonVisibilityOverride = null;
        public AppViewBackButtonVisibility? AppViewBackButtonVisibilityOverride
        {
            get => this.appViewBackButtonVisibilityOverride;
            set
            {
                if (Set(ref this.appViewBackButtonVisibilityOverride, value))
                    UpdateAppViewBackButtonVisibility();
            }
        }

        private void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (GoBack())
            {
                e.Handled = true;
            }
        }

        private void destory()
        {
            this.manager.BackRequested -= manager_BackRequested;
            this.manager = null;
            this.Handlers.CollectionChanged -= handlers_CollectionChanged;
            foreach (var item in Handlers)
            {
                item.Parent = null;
            }
            this.Handlers.Clear();
            this.Handlers = null;
        }
    }
}
