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
            frame.Navigated += this.Frame_Navigated;
        }

        private void Frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (this.Frame == null)
            {
                var f = (Frame)sender;
                f.Navigated -= this.Frame_Navigated;
                return;
            }
            this.RaiseCanGoBackChanged();
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

        public Navigator Parent { get; set; }

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
            this.RaiseCanGoBackChanged();
        }
    }
}
