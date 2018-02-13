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

namespace Opportunity.MvvmUniverse.Views
{
    public sealed class Navigator : ObservableObject
    {
        internal static readonly Dictionary<int, Navigator> NavigatorDictionary = new Dictionary<int, Navigator>();

        private static KeyValuePair<int, Navigator> cache;

        public static Navigator GetOrCreateForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (cache.Key == id)
                return cache.Value;
            if (NavigatorDictionary.TryGetValue(id, out var r))
                return r;
            return NavigatorDictionary[id] = new Navigator();
        }

        public static Navigator GetForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (cache.Key == id)
                return cache.Value;
            if (NavigatorDictionary.TryGetValue(id, out var r))
            {
                cache = new KeyValuePair<int, Navigator>(id, r);
                return r;
            }
            return null;
        }

        public static bool DestoryForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (cache.Key == id)
                cache = default;
            if (!NavigatorDictionary.TryGetValue(id, out var r))
                return false;
            NavigatorDictionary.Remove(id);
            r.destory();
            return true;
        }

        public NavigationHandlerCollection Handlers { get; }

        public SystemNavigationManager NavigationManager { get; } = SystemNavigationManager.GetForCurrentView();

        private Navigator()
        {
            this.Handlers = new NavigationHandlerCollection(this);
            this.NavigationManager.BackRequested += this.manager_BackRequested;
        }

        private void destory()
        {
            this.NavigationManager.BackRequested -= this.manager_BackRequested;
            this.Handlers.Clear();
        }

        public void UpdateAppViewBackButtonVisibility()
        {
            var ov = AppViewBackButtonVisibility;
            var nv = this.appViewBackButtonVisibilityOverride
                ?? (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            if (ov != nv)
            {
                AppViewBackButtonVisibility = nv;
                OnPropertyChanged(nameof(AppViewBackButtonVisibility));
            }
        }

        public bool CanGoBack()
        {
            if (!this.isEnabled)
                return false;
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                if (Handlers[i].CanGoBack())
                {
                    return true;
                }
            }
            return false;
        }

        public bool GoBack()
        {
            if (!this.isEnabled)
                return false;
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                var h = Handlers[i];
                if (h.CanGoBack())
                {
                    h.GoBack();
                    UpdateAppViewBackButtonVisibility();
                    return true;
                }
            }
            return false;
        }

        public bool CanGoForward()
        {
            if (!this.isEnabled)
                return false;
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                if (Handlers[i].CanGoForward())
                {
                    return true;
                }
            }
            return false;
        }

        public bool GoForward()
        {
            if (!this.isEnabled)
                return false;
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                var h = Handlers[i];
                if (h.CanGoForward())
                {
                    h.GoForward();
                    UpdateAppViewBackButtonVisibility();
                    return true;
                }
            }
            return false;
        }

        public bool Navigate(Type sourcePageType) => this.Navigate(sourcePageType, null);

        public bool Navigate(Type sourcePageType, object parameter)
        {
            if (sourcePageType == null)
                throw new ArgumentNullException(nameof(sourcePageType));
            if (!this.isEnabled)
                return false;
            for (var i = Handlers.Count - 1; i >= 0; i--)
            {
                var h = Handlers[i];
                if (h.Navigate(sourcePageType, parameter))
                {
                    UpdateAppViewBackButtonVisibility();
                    return true;
                }
            }
            return false;
        }

        public AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get => NavigationManager.AppViewBackButtonVisibility;
            private set => NavigationManager.AppViewBackButtonVisibility = value;
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

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (Set(ref this.isEnabled, value))
                    UpdateAppViewBackButtonVisibility();
            }
        }

        private void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (GoBack())
                e.Handled = true;
        }
    }
}
