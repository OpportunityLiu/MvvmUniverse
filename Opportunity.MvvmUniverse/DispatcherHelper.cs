using Opportunity.MvvmUniverse.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
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

        private static CoreDispatcher dispatcher;

        public static bool Initialized => dispatcher != null;

        public static CoreDispatcher Dispatcher => dispatcher;

        public static bool Initialize()
        {
            dispatcher = Window.Current?.Dispatcher;
            return dispatcher != null;
        }

        public static void Initialize(CoreDispatcher dispatcher)
        {
            DispatcherHelper.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public static void Uninitialize()
        {
            dispatcher = null;
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
            if (dispatcher == null)
            {
                action();
                return;
            }
            var task = dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
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
            if (dispatcher == null || dispatcher.HasThreadAccess)
            {
                action();
                return AsyncWrapper.CreateCompleted();
            }
            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }
    }
}
