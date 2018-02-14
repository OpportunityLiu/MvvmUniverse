using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Opportunity.MvvmUniverse.Views
{
    public class FrameWrapper : INavigationHandler
    {
        public FrameWrapper(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            this.frame = new WeakReference<Frame>(frame);
        }

        private readonly WeakReference<Frame> frame;

        public Frame Frame
        {
            get
            {
                if (this.frame.TryGetTarget(out var t))
                    return t;
                return null;
            }
        }

        public bool CanGoBack()
        {
            var f = Frame;
            if (f == null)
                return false;
            return f.CanGoBack;
        }

        public IAsyncOperation<bool> GoBackAsync()
        {
            var f = Frame;
            if (f == null)
                return AsyncWrapper.CreateCompleted(false);
            if (!f.CanGoBack)
                return AsyncWrapper.CreateCompleted(false);
            f.GoBack();
            return AsyncWrapper.CreateCompleted(true);
        }

        public bool CanGoForward()
        {
            var f = Frame;
            if (f == null)
                return false;
            return f.CanGoForward;
        }

        public IAsyncOperation<bool> GoForwardAsync()
        {
            var f = Frame;
            if (f == null)
                return AsyncWrapper.CreateCompleted(false);
            if (!f.CanGoForward)
                return AsyncWrapper.CreateCompleted(false);
            f.GoForward();
            return AsyncWrapper.CreateCompleted(true);
        }

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter)
        {
            var f = Frame;
            if (f == null)
                return AsyncWrapper.CreateCompleted(false);
            return AsyncInfo.Run(async token =>
            {
                if (!f.Navigate(sourcePageType, parameter))
                    return false;
                await f.Dispatcher.YieldIdle();
                return true;
            });
        }
    }
}
