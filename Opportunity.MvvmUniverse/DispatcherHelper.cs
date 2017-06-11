using Opportunity.MvvmUniverse.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse
{
    public static class DispatcherHelper
    {
        internal static int GetCurrentViewId()
        {
            var window = Window.Current;
            if (window == null)
                throw new InvalidOperationException("Wrong thread, this method can be only invoked in UI thread.");
            return ApplicationView.GetApplicationViewIdForWindow(window.CoreWindow);
        }

        public static bool Initialized => Dispatcher != null;

        public static CoreDispatcher Dispatcher { get; private set; }

        public static bool Initialize()
        {
            Dispatcher = Window.Current?.Dispatcher;
            return Initialized;
        }

        public static void Initialize(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public static void Uninitialize()
        {
            Dispatcher = null;
        }

        public static bool UseForNotification { get; set; } = false;

        public static void BeginInvoke(DispatchedHandler action)
        {
            if (UseForNotification)
                BeginInvokeOnUIThread(action);
            else
                action();
        }

        public static void BeginInvokeOnUIThread(DispatchedHandler action)
        {
            if (Dispatcher == null)
            {
                action();
                return;
            }
            Dispatcher.Begin(action, CoreDispatcherPriority.Normal);
        }

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

        public static IAsyncAction RunAsyncOnUIThread(DispatchedHandler action)
        {
            if (Dispatcher == null || Dispatcher.HasThreadAccess)
            {
                action();
                return AsyncWrapper.CreateCompleted();
            }
            return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }

        public static DispatcherAwaiterSource Yield()
        {
            return new DispatcherAwaiterSource(Dispatcher, CoreDispatcherPriority.Normal);
        }

        public static DispatcherAwaiterSource Yield(CoreDispatcherPriority priority)
        {
            return new DispatcherAwaiterSource(Dispatcher, priority);
        }

        public static DispatcherAwaiterSource YieldIdle()
        {
            return new DispatcherAwaiterSource(Dispatcher, CoreDispatcherPriority.Idle);
        }
    }
}
