using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Opportunity.MvvmUniverse.Views
{
    public static class FrameExtension
    {
        public static FrameWrapper AsNavigationHandler(this Frame frame) => new FrameWrapper(frame);
    }

    public class FrameWrapper : INavigationHandler
    {
        public FrameWrapper(Frame frame)
        {
            this.Frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public Frame Frame { get; }

        public bool CanGoBack() => Frame.CanGoBack;

        public IAsyncOperation<bool> GoBackAsync()
        {
            var f = Frame;
            if (!f.CanGoBack)
                return AsyncOperation<bool>.CreateCompleted(false);
            f.GoBack();
            return AsyncOperation<bool>.CreateCompleted(true);
        }

        public bool CanGoForward() => Frame.CanGoForward;

        public IAsyncOperation<bool> GoForwardAsync()
        {
            var f = Frame;
            if (!f.CanGoForward)
                return AsyncOperation<bool>.CreateCompleted(false);
            f.GoForward();
            return AsyncOperation<bool>.CreateCompleted(true);
        }

        public IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter)
        {
            var f = Frame;
            if (!f.Navigate(sourcePageType, parameter))
                return AsyncOperation<bool>.CreateCompleted(false);
            return AsyncOperation<bool>.CreateCompleted(true);
        }
    }
}
