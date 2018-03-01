﻿using System.Collections.Generic;
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
using Opportunity.MvvmUniverse.Commands;

namespace Opportunity.MvvmUniverse.Views
{
    public sealed class Navigator : ObservableObject
    {
        internal static readonly Dictionary<int, Navigator> NavigatorDictionary = new Dictionary<int, Navigator>();

        private static Tuple<int, Navigator> cache;

        public static Navigator GetOrCreateForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            var cache = Navigator.cache;
            if (cache != null && cache.Item1 == id)
                return cache.Item2;
            if (NavigatorDictionary.TryGetValue(id, out var r))
                return r;
            lock (NavigatorDictionary)
            {
                var nav = new Navigator();
                NavigatorDictionary[id] = nav;
                Navigator.cache = Tuple.Create(id, nav);
                return nav;
            }
        }

        public static Navigator GetForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            var cache = Navigator.cache;
            if (cache != null && cache.Item1 == id)
                return cache.Item2;
            if (NavigatorDictionary.TryGetValue(id, out var r))
            {
                Navigator.cache = Tuple.Create(id, r);
                return r;
            }
            return null;
        }

        public static bool DestoryForCurrentView()
        {
            var id = DispatcherHelper.GetCurrentViewId();
            if (!NavigatorDictionary.TryGetValue(id, out var r))
                return false;
            lock (NavigatorDictionary)
            {
                var cache = Navigator.cache;
                if (cache != null && cache.Item1 == id)
                    Navigator.cache = null;
                NavigatorDictionary.Remove(id);
                r.destory();
                return true;
            }
        }

        public NavigationHandlerCollection Handlers { get; private set; }

        public SystemNavigationManager NavigationManager { get; private set; } = SystemNavigationManager.GetForCurrentView();

        public AppViewBackButtonVisibility AppViewBackButtonVisibility
        {
            get
            {
                CheckAvailable();
                return NavigationManager.AppViewBackButtonVisibility;
            }
            private set => NavigationManager.AppViewBackButtonVisibility = value;
        }

        private async void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CanGoBack())
            {
                e.Handled = true;
                await GoBackAsync();
            }
        }

        public void UpdateAppViewBackButtonVisibility()
        {
            var ov = AppViewBackButtonVisibility;
            var nv = (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            if (ov != nv)
            {
                AppViewBackButtonVisibility = nv;
                OnPropertyChanged(nameof(AppViewBackButtonVisibility));
            }
        }

        private Navigator()
        {
            this.Handlers = new NavigationHandlerCollection(this);
            this.NavigationManager.BackRequested += this.manager_BackRequested;
        }

        private bool destoryed = false;
        private void destory()
        {
            if (!this.destoryed)
            {
                this.NavigationManager.BackRequested -= this.manager_BackRequested;
                this.Handlers.Clear();
                this.Handlers = null;
                this.NavigationManager = null;
                this.destoryed = true;
            }
        }

        private void CheckAvailable()
        {
            if (this.destoryed)
                throw new InvalidOperationException("This navigator has been destoryed.");
        }

        private bool navigating;
        public bool Navigating { get => this.navigating; private set => Set(ref this.navigating, value); }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                CheckAvailable();
                if (Set(ref this.isEnabled, value))
                    UpdateAppViewBackButtonVisibility();
            }
        }

        private bool isGoBackEnabled = true;
        public bool IsGoBackEnabled
        {
            get => this.isGoBackEnabled;
            set
            {
                CheckAvailable();
                if (Set(ref this.isGoBackEnabled, value))
                    UpdateAppViewBackButtonVisibility();
            }
        }

        public bool CanGoBack()
        {
            CheckAvailable();
            if (!this.isEnabled || !this.isGoBackEnabled)
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
            CheckAvailable();
            if (!this.isEnabled || !this.isGoBackEnabled || this.navigating)
                return AsyncOperation<bool>.CreateCompleted(false);
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
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    UpdateAppViewBackButtonVisibility();
                    this.Navigating = false;
                }
            });
        }

        private bool isGoForwardEnabled = true;
        public bool IsGoForwardEnabled
        {
            get => this.isGoForwardEnabled;
            set
            {
                CheckAvailable();
                Set(ref this.isGoForwardEnabled, value);
            }
        }

        public bool CanGoForward()
        {
            CheckAvailable();
            if (!this.isEnabled || !this.isGoForwardEnabled)
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
            CheckAvailable();
            if (!this.isEnabled || !this.isGoForwardEnabled || this.navigating)
                return AsyncOperation<bool>.CreateCompleted(false);
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
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    UpdateAppViewBackButtonVisibility();
                    this.Navigating = false;
                }
            });
        }

        private bool isNavigateEnabled = true;
        public bool IsNavigateEnabled
        {
            get => this.isNavigateEnabled;
            set
            {
                CheckAvailable();
                Set(ref this.isNavigateEnabled, value);
            }
        }

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType) => this.NavigateAsync(sourcePageType, null);

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter)
        {
            CheckAvailable();
            if (sourcePageType == null)
                throw new ArgumentNullException(nameof(sourcePageType));
            if (!this.isEnabled || !this.isNavigateEnabled || this.navigating)
                return AsyncOperation<bool>.CreateCompleted(false);
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
                            return true;
                        }
                    }
                    return false;
                }
                finally
                {
                    UpdateAppViewBackButtonVisibility();
                    this.Navigating = false;
                }
            });
        }
    }
}
