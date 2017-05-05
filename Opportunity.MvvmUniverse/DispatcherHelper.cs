using Opportunity.MvvmUniverse.AsyncWrappers;
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

        public static void Init()
        {
            dispatcher = Window.Current?.Dispatcher;
        }

        public static void Init(CoreDispatcher dispatcher)
        {
            DispatcherHelper.dispatcher = dispatcher;
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
            if (dispatcher == null || dispatcher.HasThreadAccess)
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
            action();
            return AsyncWrapper.CreateCompleted();
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
