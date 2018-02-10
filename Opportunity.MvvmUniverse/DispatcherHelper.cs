using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse
{
    /// <summary>
    /// Static class that stores a <see cref="CoreDispatcher"/> used by callbacks in this lib.
    /// </summary>
    public static class DispatcherHelper
    {
        internal static int GetCurrentViewId()
        {
            var window = Window.Current;
            if (window == null)
                throw new InvalidOperationException("Wrong thread, this method can be only invoked on UI thread.");
            return ApplicationView.GetApplicationViewIdForWindow(window.CoreWindow);
        }

        /// <summary>
        /// Check if this helper has been initialized, call <see cref="Initialize()"/> to initialize the helper.
        /// </summary>
        public static bool Initialized => Dispatcher != null;

        /// <summary>
        /// <see cref="CoreDispatcher"/> stored in the helper.
        /// </summary>
        public static CoreDispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Use <see cref="CoreDispatcher"/> of current thread to initialize.
        /// Must be called on UI thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Method called on a non-UI thread.</exception>
        public static void Initialize()
        {
            var d = Window.Current;
            if (d == null)
                throw new InvalidOperationException("Must be called on UI thread");
            Initialize(d.Dispatcher);
        }

        /// <summary>
        /// Use specified <see cref="CoreDispatcher"/> to initialize.
        /// </summary>
        /// <param name="dispatcher"><see cref="CoreDispatcher"/> to store in the helper</param>
        /// <exception cref="ArgumentNullException"><paramref name="dispatcher"/> is <see langword="null"/></exception>
        public static void Initialize(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// Set <see cref="Dispatcher"/> to <see langword="null"/>.
        /// </summary>
        public static void Uninitialize()
        {
            Dispatcher = null;
        }

        /// <summary>
        /// Should objects like <see cref="ObservableObject"/> in this lib use the helper to invoke callbacks on UI thread.
        /// Default value is false.
        /// </summary>
        public static bool UseForNotification { get; set; } = false;

        /// <summary>
        /// Begin invoke of <paramref name="action"/>.
        /// </summary>
        /// <param name="action">delegate to invoke</param>
        /// <remarks>Will check <see cref="UseForNotification"/> to decide the thread of invocation.</remarks>
        public static void BeginInvoke(DispatchedHandler action)
        {
            if (UseForNotification)
                BeginInvokeOnUIThread(action);
            else
                action();
        }

        /// <summary>
        /// Begin invoke of <paramref name="action"/> on UI thread and returns immediately.
        /// </summary>
        /// <param name="action">delegate to invoke</param>
        /// <remarks>If not <see cref="Initialized"/>, <paramref name="action"/> will be invoked synchronously.</remarks>
        public static void BeginInvokeOnUIThread(DispatchedHandler action)
        {
            if (Dispatcher == null)
            {
                action();
                return;
            }
            Dispatcher.Begin(action, CoreDispatcherPriority.Normal);
        }

        /// <summary>
        /// Invoke <paramref name="action"/>.
        /// </summary>
        /// <param name="action">delegate to invoke</param>
        /// <remarks>Will check <see cref="UseForNotification"/> to decide the thread of invocation.</remarks>
        public static IAsyncAction RunAsync(DispatchedHandler action)
        {
            if (UseForNotification)
                return RunAsyncOnUIThread(action);
            else
            {
                action();
                return AsyncWrapper.CreateCompleted();
            }
        }

        /// <summary>
        /// Invoke <paramref name="action"/> on UI thread and returns immediately.
        /// </summary>
        /// <param name="action">delegate to invoke</param>
        /// <remarks>If not <see cref="Initialized"/>, or called on UI thread, <paramref name="action"/> will be invoked synchronously.</remarks>
        public static IAsyncAction RunAsyncOnUIThread(DispatchedHandler action)
        {
            if (Dispatcher == null || Dispatcher.HasThreadAccess)
            {
                action();
                return AsyncWrapper.CreateCompleted();
            }
            return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Change context to UI thread of <see cref="Dispatcher"/>.
        /// </summary>
        /// <returns>Awaiter of yield</returns>
        public static DispatcherAwaiterSource Yield() => Dispatcher.Yield();

        /// <summary>
        /// Change context to UI thread of <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="priority">priority of context changing</param>
        /// <returns>Awaiter of yield</returns>
        public static DispatcherAwaiterSource Yield(CoreDispatcherPriority priority) => Dispatcher.Yield(priority);

        /// <summary>
        /// Change context to UI thread of <see cref="Dispatcher"/> with idle priority.
        /// </summary>
        /// <returns>Awaiter of yield</returns>
        public static DispatcherAwaiterSource YieldIdle() => Dispatcher.YieldIdle();
    }
}
