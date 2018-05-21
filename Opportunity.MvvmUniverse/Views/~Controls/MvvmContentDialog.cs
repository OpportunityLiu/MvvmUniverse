﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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
        private bool opening = false;
        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
            this.BackgroundElement = GetTemplateChild(nameof(this.BackgroundElement)) as Border;
            if (this.BackgroundElement != null && this.opening)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
            this.DialogSpace = GetTemplateChild(nameof(this.DialogSpace)) as Grid;
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void MvvmContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneChanging;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged += this.MvvmContentDialog_VisibleBoundsChanged;
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
            this.opening = true;
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void MvvmContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneChanging;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged -= this.MvvmContentDialog_VisibleBoundsChanged;
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
            this.ClearValue(WidthProperty);
            this.ClearValue(HeightProperty);
            this.opening = false;
        }

        private void InputPane_InputPaneChanging(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = false;
        }

        private void BackgroundElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine($"BackgroundElement_SizeChanged {e.NewSize}");
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private async void MvvmContentDialog_VisibleBoundsChanged(object sender, Rect e)
        {
            Debug.WriteLine($"MvvmContentDialog_VisibleBoundsChanged {e}");
            caculateVisibleBoundsThickness(e);
            await Task.Delay(16);
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void caculateVisibleBoundsThickness(Rect vb)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            if (this.BackgroundElement is null || this.DialogSpace is null)
                return;
            var size = new Size(this.BackgroundElement.ActualWidth, this.BackgroundElement.ActualHeight);
            var transedView = this.BackgroundElement.TransformToVisual(null).Inverse.TransformBounds(vb);
            var innerBound = this.DialogSpace.Padding;
            var padding = new Thickness(
                (transedView.Left - innerBound.Left).BoundToZero(),
                (transedView.Top - innerBound.Top).BoundToZero(),
                (size.Width - transedView.Right - innerBound.Right).BoundToZero(),
                (size.Height - transedView.Bottom - innerBound.Bottom).BoundToZero());
            if (MathHelper.Diff(this.BackgroundElement.Padding, padding) < 4)
                return;
            Debug.WriteLine($"New padding {padding}");
            this.BackgroundElement.Padding = padding;
        }
    }
}
