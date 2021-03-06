﻿using Opportunity.Helpers.ObjectModel;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse.Services.Navigation
{
    /// <summary>
    /// Provides view level navigation service.
    /// </summary>
    public sealed class Navigator : DependencyServiceBase<Navigator, INavigationHandler>
    {
        /// <summary>
        /// Create or get <see cref="Navigator"/> of current view.
        /// </summary>
        /// <returns><see cref="Navigator"/> of current view.</returns>
        public static Navigator GetOrCreateForCurrentView()
            => ThreadLocalSingleton.GetOrCreate(() => new Navigator());

        /// <summary>
        /// Get <see cref="Navigator"/> of current view.
        /// </summary>
        /// <returns><see cref="Navigator"/> of current view, or <see langword="null"/>, if not created.</returns>
        public static Navigator GetForCurrentView() => ThreadLocalSingleton.Get<Navigator>();

        /// <summary>
        /// Destory <see cref="Navigator"/> of current view.
        /// </summary>
        /// <returns>Whether the <see cref="Navigator"/> is found and destoryed.</returns>
        public static bool DestoryForCurrentView()
        {
            var nav = ThreadLocalSingleton.Reset<Navigator>();
            if (nav is null)
                return false;
            nav.destory();
            return true;
        }

        private Navigator()
        {
            this.SystemNavigationManager = SystemNavigationManager.GetForCurrentView();
            this.SystemNavigationManager.BackRequested += this.manager_BackRequested;
        }

        private void destory()
        {
            if (this.Handlers.Service != null)
            {
                this.SystemNavigationManager.BackRequested -= this.manager_BackRequested;
                this.SystemNavigationManager = null;
                this.Handlers.Dispose();
            }
        }

        /// <summary>
        /// <see cref="Windows.UI.Core.SystemNavigationManager"/> of this instance.
        /// </summary>
        public SystemNavigationManager SystemNavigationManager { get; private set; }

        private async void manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (e.Handled)
                return;
            if (CanGoBack)
            {
                e.Handled = true;
                await GoBackAsync();
            }
        }

        private void CheckAvailable()
        {
            if (this.Handlers.Service is null)
                throw new InvalidOperationException("This navigator has been destoryed.");
        }

        /// <summary>
        /// Manually caculates and updates values of <see cref="CanGoBack"/> and <see cref="CanGoForward"/>.
        /// </summary>
        protected override void UpdatePropertiesOverride()
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
        /// <summary>
        /// Indicates if <see cref="GoBackAsync()"/>, <see cref="GoForwardAsync()"/> or <see cref="NavigateAsync(Type, object)"/> is running.
        /// </summary>
        public bool IsNavigating
        {
            get => this.isNavigating;
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

        /// <summary>
        /// Global switch of this <see cref="Navigator"/>.
        /// </summary>
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

        /// <summary>
        /// Indicates whether <see cref="GoBackAsync()"/> is enabled.
        /// </summary>
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

        /// <summary>
        /// Indicates whether <see cref="GoForwardAsync()"/> is enabled.
        /// </summary>
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

        /// <summary>
        /// Indicates whether <see cref="NavigateAsync(Type, object)"/> is enabled.
        /// </summary>
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
        /// <summary>
        /// Indicates if can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get => this.canGoBack;
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
        /// <summary>
        /// Indicates if can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get => this.canGoForward;
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

        /// <summary>
        /// Navagate back.
        /// </summary>
        /// <returns>Navigation succeed or not.</returns>
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

        /// <summary>
        /// Navagate forward.
        /// </summary>
        /// <returns>Navigation succeed or not.</returns>
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

        /// <summary>
        /// Navagate to new page.
        /// </summary>
        /// <param name="sourcePageType">Type of new page.</param>
        /// <returns>Navigation succeed or not.</returns>
        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType) => this.NavigateAsync(sourcePageType, null);

        /// <summary>
        /// Navagate to new page.
        /// </summary>
        /// <param name="sourcePageType">Type of new page.</param>
        /// <param name="parameter">Parameter of navigation.</param>
        /// <returns>Navigation succeed or not.</returns>
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
