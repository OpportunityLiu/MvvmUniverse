using System;
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

        public void GoBack()
        {
            var f = Frame;
            if (f == null)
                return;
            f.GoBack();
        }

        public bool CanGoForward()
        {
            var f = Frame;
            if (f == null)
                return false;
            return f.CanGoForward;
        }

        public void GoForward()
        {
            var f = Frame;
            if (f == null)
                return;
            f.GoForward();
        }

        public bool Navigate(Type sourcePageType, object parameter)
        {
            var f = Frame;
            if (f == null)
                return false;
            return f.Navigate(sourcePageType, parameter);
        }
    }
}
