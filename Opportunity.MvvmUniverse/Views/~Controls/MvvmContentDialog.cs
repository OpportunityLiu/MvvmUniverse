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
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void MvvmContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneChanging;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged += this.MvvmContentDialog_VisibleBoundsChanged;
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
        }

        private void MvvmContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneChanging;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged -= this.MvvmContentDialog_VisibleBoundsChanged;
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
            this.ClearValue(WidthProperty);
            this.ClearValue(HeightProperty);
        }

        private void InputPane_InputPaneChanging(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = false;
        }

        private void BackgroundElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void MvvmContentDialog_VisibleBoundsChanged(object sender, Rect e)
        {
            caculateVisibleBoundsThickness(e);
        }

        private void caculateVisibleBoundsThickness(Rect vb)
        {
            if (this.BackgroundElement != null)
            {
                var size = new Size(this.BackgroundElement.ActualWidth, this.BackgroundElement.ActualHeight);
                var transedView = this.BackgroundElement.TransformToVisual(null).Inverse.TransformBounds(vb);
                var innerBound = this.DialogSpace.Padding;
                var padding = new Thickness(bound(transedView.Left - innerBound.Left), bound(transedView.Top - innerBound.Top), bound(size.Width - transedView.Right - innerBound.Right), bound(size.Height - transedView.Bottom - innerBound.Bottom));
                this.BackgroundElement.Padding = padding;
            }
        }

        private static double bound(double v) => v < 0 ? 0 : v;
    }
}
