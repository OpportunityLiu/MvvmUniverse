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
using Opportunity.MvvmUniverse.Commands;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace Opportunity.MvvmUniverse.Views
{
    public sealed class Navigator : DependencyObject
    {
        internal static int Count;
        [ThreadStatic]
        private static Navigator navigator;

        public static Navigator GetOrCreateForCurrentView()
        {
            var nav = navigator;
            if (nav != null)
                return nav;
            nav = new Navigator();
            navigator = nav;
            return nav;
        }

        public static Navigator GetForCurrentView() => navigator;

        public static bool DestoryForCurrentView()
        {
            var nav = Interlocked.Exchange(ref navigator, null);
            if (nav == null)
                return false;
            nav.destory();
            return true;
        }

        public NavigationHandlerCollection Handlers { get; private set; }

        public SystemNavigationManager SystemNavigationManager { get; private set; }

        private async void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CanGoBack)
            {
                e.Handled = true;
                await GoBackAsync();
            }
        }

        private Navigator()
        {
            Interlocked.Increment(ref Count);
            this.Handlers = new NavigationHandlerCollection(this);
            this.SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
            this.SystemNavigationManager.BackRequested += this.manager_BackRequested;
        }

        private bool destoryed = false;
        private void destory()
        {
            if (!this.destoryed)
            {
                this.SystemNavigationManager.BackRequested -= this.manager_BackRequested;
                this.SystemNavigationManager = null;
                this.Handlers.Clear();
                this.Handlers = null;
                this.destoryed = true;
                Interlocked.Decrement(ref Count);
            }
        }

        private void CheckAvailable()
        {
            if (this.destoryed)
                throw new InvalidOperationException("This navigator has been destoryed.");
        }

        public void UpdateProperties()
        {
            CheckAvailable();
            var canBack = false;
            var canForward = false;
            if (this.IsEnabled)
            {
                var be = this.IsBackEnabled;
                var fe = this.IsForwardEnabled;
                if (be || fe)
                {
                    for (var i = Handlers.Count - 1; i >= 0; i--)
                    {
                        if (be && !canBack && Handlers[i].CanGoBack)
                            canBack = true;
                        if (fe && !canForward && Handlers[i].CanGoForward)
                            canForward = true;
                        if (canBack && canForward)
                            break;
                    }
                }
            }
            this.CanGoBack = canBack;
            this.CanGoForward = canForward;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool isNavigating = false;
        public bool IsNavigating
        {
            get => (bool)GetValue(IsNavigatingProperty);
            private set
            {
                if (this.isNavigating == value)
                    return;
                this.isNavigating = value;
                SetValue(IsNavigatingProperty, value);
            }
        }

        /// <summary>
        /// Indentify <see cref="IsNavigating"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsNavigatingProperty =
            DependencyProperty.Register(nameof(IsNavigating), typeof(bool), typeof(Navigator), new PropertyMetadata(false, IsNavigatingPropertyChanged));

        private static void IsNavigatingPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;
            if (sender.isNavigating != newValue)
                throw new InvalidOperationException("This property is read only");
        }

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(Navigator), new PropertyMetadata(true, IsEnabledPropertyChanged));

        private static void IsEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;
            sender.UpdateProperties();
        }

        public bool IsBackEnabled
        {
            get => (bool)GetValue(IsBackEnabledProperty);
            set => SetValue(IsBackEnabledProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="IsBackEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(nameof(IsBackEnabled), typeof(bool), typeof(Navigator), new PropertyMetadata(true, IsBackEnabledPropertyChanged));

        private static void IsBackEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;
            sender.UpdateProperties();
        }

        public bool IsForwardEnabled
        {
            get => (bool)GetValue(IsForwardEnabledProperty);
            set => SetValue(IsForwardEnabledProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="IsForwardEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsForwardEnabledProperty =
            DependencyProperty.Register(nameof(IsForwardEnabled), typeof(bool), typeof(Navigator), new PropertyMetadata(true, IsForwardEnabledPropertyChanged));

        private static void IsForwardEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;
            sender.UpdateProperties();
        }

        public bool IsNavigateEnabled
        {
            get => (bool)GetValue(IsNavigateEnabledProperty);
            set => SetValue(IsNavigateEnabledProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="IsNavigateEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsNavigateEnabledProperty =
            DependencyProperty.Register(nameof(IsNavigateEnabled), typeof(bool), typeof(Navigator), new PropertyMetadata(true, IsNavigateEnabledPropertyChanged));

        private static void IsNavigateEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;

        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool canGoBack = false;
        public bool CanGoBack
        {
            get => (bool)GetValue(CanGoBackProperty);
            private set
            {
                if (this.canGoBack == value)
                    return;
                this.canGoBack = value;
                SetValue(CanGoBackProperty, value);
            }
        }

        /// <summary>
        /// Indentify <see cref="CanGoBack"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanGoBackProperty =
            DependencyProperty.Register(nameof(CanGoBack), typeof(bool), typeof(Navigator), new PropertyMetadata(false, CanGoBackPropertyChanged));

        private static void CanGoBackPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;
            if (sender.canGoBack != newValue)
                throw new InvalidOperationException("This property is read only");
            sender.SystemNavigationManager.AppViewBackButtonVisibility
                = newValue ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool canGoForward = false;
        public bool CanGoForward
        {
            get => (bool)GetValue(CanGoForwardProperty);
            private set
            {
                if (this.canGoForward == value)
                    return;
                this.canGoForward = value;
                SetValue(CanGoForwardProperty, value);
            }
        }

        /// <summary>
        /// Indentify <see cref="CanGoForward"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanGoForwardProperty =
            DependencyProperty.Register(nameof(CanGoForward), typeof(bool), typeof(Navigator), new PropertyMetadata(false, CanGoForwardPropertyChanged));

        private static void CanGoForwardPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Navigator)dp;
            if (sender.canGoForward != newValue)
                throw new InvalidOperationException("This property is read only");
        }

        public IAsyncOperation<bool> GoBackAsync()
        {
            CheckAvailable();
            if (!this.IsEnabled || !this.IsBackEnabled || this.isNavigating)
                return AsyncOperation<bool>.CreateCompleted(false);
            this.IsNavigating = true;
            return AsyncInfo.Run(async token =>
            {
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
                    UpdateProperties();
                    this.IsNavigating = false;
                }
            });
        }

        public IAsyncOperation<bool> GoForwardAsync()
        {
            CheckAvailable();
            if (!this.IsEnabled || !this.IsForwardEnabled || this.isNavigating)
                return AsyncOperation<bool>.CreateCompleted(false);
            this.IsNavigating = true;
            return AsyncInfo.Run(async token =>
            {
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
                    UpdateProperties();
                    this.IsNavigating = false;
                }
            });
        }

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType) => this.NavigateAsync(sourcePageType, null);

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter)
        {
            CheckAvailable();
            if (sourcePageType == null)
                throw new ArgumentNullException(nameof(sourcePageType));
            if (!this.IsEnabled || !this.IsNavigateEnabled || this.isNavigating)
                return AsyncOperation<bool>.CreateCompleted(false);
            this.IsNavigating = true;
            return AsyncInfo.Run(async token =>
            {
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
                    UpdateProperties();
                    this.IsNavigating = false;
                }
            });
        }
    }
}
