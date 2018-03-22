using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Opportunity.MvvmUniverse.Views
{
    internal class VisibleBoundsHelper
    {
        public static VisibleBoundsHelper GetForCurrentView()
        {
            if (CoreApplication.GetCurrentView().Properties.TryGetValue(nameof(VisibleBoundsHelper), out var helper))
                return (VisibleBoundsHelper)helper;
            var tHelper = new VisibleBoundsHelper();
            tHelper.caculateVisibleBoundsThickness();
            CoreApplication.GetCurrentView().Properties[nameof(VisibleBoundsHelper)] = tHelper;
            return tHelper;
        }

        private VisibleBoundsHelper() { }

        private EventHandler<Rect> visibleBoundsChanged;
        public event EventHandler<Rect> VisibleBoundsChanged
        {
            add
            {
                var on = this.visibleBoundsChanged is null;
                this.visibleBoundsChanged += value;
                if (on && this.visibleBoundsChanged != null)
                    initialize();
            }
            remove
            {
                var on = this.visibleBoundsChanged is null;
                this.visibleBoundsChanged -= value;
                if (!on && this.visibleBoundsChanged is null)
                    uninitialize();
            }
        }

        private void initialize()
        {
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneShowing;
            InputPane.GetForCurrentView().Hiding += this.InputPane_InputPaneHiding;
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += this.ApplicationView_VisibleBoundsChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += this.TitleBar_LayoutMetricsChanged;
            this.timer.Tick += this.Timer_Tick;
            this.timer.Start();
        }

        private void uninitialize()
        {
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneShowing;
            InputPane.GetForCurrentView().Hiding -= this.InputPane_InputPaneHiding;
            ApplicationView.GetForCurrentView().VisibleBoundsChanged -= this.ApplicationView_VisibleBoundsChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= this.TitleBar_LayoutMetricsChanged;
            this.timer.Tick -= this.Timer_Tick;
            this.timer.Stop();
        }

        private DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };

        private void Timer_Tick(object sender, object e)
        {
            caculateVisibleBoundsThickness();
        }

        private void InputPane_InputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            caculateVisibleBoundsThickness();
        }

        private void InputPane_InputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            caculateVisibleBoundsThickness();
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            caculateVisibleBoundsThickness();
        }

        private void ApplicationView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            caculateVisibleBoundsThickness();
        }

        private void caculateVisibleBoundsThickness()
        {
            var paneRect = InputPane.GetForCurrentView().OccludedRect;
            if (paneRect.Width == 0 || paneRect.Height == 0)
                paneRect = Rect.Empty;

            var coreView = CoreApplication.GetCurrentView();
            var applicationView = ApplicationView.GetForCurrentView();
            var isFullScreen = applicationView.IsFullScreenMode;
            var isTablet = UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch;
            var tb = coreView.TitleBar;
            var tbh = (tb.ExtendViewIntoTitleBar && !(isFullScreen || isTablet)) ? tb.Height : 0;
            var wb = CoreWindow.GetForCurrentThread().Bounds;
            var vb = isFullScreen ? wb : applicationView.VisibleBounds;

            var left = 0d;
            var top = 0d;
            var width = 0d;
            var height = 0d;

            left = vb.Left - wb.Left;
            top = vb.Top + tbh - wb.Top;
            width = vb.Width;
            height = paneRect.IsEmpty ? (vb.Height - tbh) : (paneRect.Top - top);

            VisibleBounds = new Rect(left, top, width, height);
        }

        private Rect visibleBounds = Rect.Empty;
        public Rect VisibleBounds
        {
            get => this.visibleBounds;
            private set
            {
                var old = this.visibleBounds;
                if (old == value)
                    return;
                this.visibleBounds = value;
                this.visibleBoundsChanged?.Invoke(this, value);
            }
        }

        private static double bound(double v) => v < 0 ? 0 : v;
    }
}
