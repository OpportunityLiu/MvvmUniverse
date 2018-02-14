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
using Windows.Foundation;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Runtime.InteropServices.WindowsRuntime;

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

        private bool navigating;
        public bool Navigating { get => this.navigating; set => Set(ref this.navigating, value); }

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

        public IAsyncOperation<bool> GoBackAsync()
        {
            if (!this.isEnabled || this.navigating)
                return AsyncWrapper.CreateCompleted(false);
            return AsyncInfo.Run(async token =>
            {
                this.Navigating = true;
                try
                {
                    for (var i = Handlers.Count - 1; i >= 0; i--)
                    {
                        var h = Handlers[i];
                        if (await h.GoBackAsync())
                        {
                            UpdateAppViewBackButtonVisibility();
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    this.Navigating = false;
                }
            });
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

        public IAsyncOperation<bool> GoForwardAsync()
        {
            if (!this.isEnabled || this.navigating)
                return AsyncWrapper.CreateCompleted(false);
            return AsyncInfo.Run(async token =>
            {
                this.Navigating = true;
                try
                {
                    for (var i = Handlers.Count - 1; i >= 0; i--)
                    {
                        var h = Handlers[i];
                        if (await h.GoForwardAsync())
                        {
                            UpdateAppViewBackButtonVisibility();
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    this.Navigating = false;
                }
            });
        }

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType) => this.NavigateAsync(sourcePageType, null);

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter)
        {
            if (sourcePageType == null)
                throw new ArgumentNullException(nameof(sourcePageType));
            if (!this.isEnabled || this.navigating)
                return AsyncWrapper.CreateCompleted(false);
            return AsyncInfo.Run(async token =>
            {
                this.Navigating = true;
                try
                {
                    for (var i = Handlers.Count - 1; i >= 0; i--)
                    {
                        var h = Handlers[i];
                        if (await h.NavigateAsync(sourcePageType, parameter))
                        {
                            UpdateAppViewBackButtonVisibility();
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    this.Navigating = false;
                }
            });
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

        private async void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CanGoBack())
            {
                e.Handled = true;
                await GoBackAsync();
            }
        }
    }
}
