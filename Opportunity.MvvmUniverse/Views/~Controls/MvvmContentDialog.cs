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
using Windows.UI.Xaml.Controls;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// <see cref="ContentDialog"/> with visible bounds awareness.
    /// </summary>
    public class MvvmContentDialog : ContentDialog
    {
        /// <summary>
        /// Create new instance of <see cref="MvvmContentDialog"/>.
        /// </summary>
        public MvvmContentDialog()
        {
            this.Opened += this.MvvmContentDialog_Opened;
            this.Closed += this.MvvmContentDialog_Closed;
        }

        private Border BackgroundElement;
        private Grid DialogSpace;
        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
            this.BackgroundElement = (Border)GetTemplateChild(nameof(this.BackgroundElement));
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
            this.DialogSpace = (Grid)GetTemplateChild(nameof(this.DialogSpace));
            caculateVisibleBoundsThickness();
        }

        private void MvvmContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneChanging;
            InputPane.GetForCurrentView().Hiding += this.InputPane_InputPaneChanging;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += this.TitleBar_LayoutMetricsChanged;
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
        }

        private void MvvmContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneChanging;
            InputPane.GetForCurrentView().Hiding -= this.InputPane_InputPaneChanging;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= this.TitleBar_LayoutMetricsChanged;
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
        }

        private void InputPane_InputPaneChanging(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = false;
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            caculateVisibleBoundsThickness();
        }

        private void BackgroundElement_SizeChanged(object sender, SizeChangedEventArgs e)
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
            var tb = coreView.TitleBar;
            var tbh = (tb.ExtendViewIntoTitleBar && !isFullScreen) ? tb.Height : 0;
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

            var usedView = new Rect(left, top, width, height);

            if (this.BackgroundElement != null)
            {
                var size = new Size(this.BackgroundElement.ActualWidth, this.BackgroundElement.ActualHeight);
                var transedView = this.BackgroundElement.TransformToVisual(null).Inverse.TransformBounds(usedView);
                var innerBound = this.DialogSpace.Padding;
                var padding = new Thickness(bound(transedView.Left - innerBound.Left), bound(transedView.Top - innerBound.Top), bound(size.Width - transedView.Right - innerBound.Right), bound(size.Height - transedView.Bottom - innerBound.Bottom));
                this.BackgroundElement.Padding = padding;
            }
        }

        private static double bound(double v) => v < 0 ? 0 : v;
    }
}
