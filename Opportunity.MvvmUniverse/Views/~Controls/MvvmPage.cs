using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Page with accociated view model and supports <see cref="VisibleBounds"/>.
    /// </summary>
    public class MvvmPage : Page
    {
        /// <summary>
        /// Create new instance of <see cref="MvvmPage"/>.
        /// </summary>
        public MvvmPage()
        {
            this.Unloaded += this.MvvmPage_Unloaded;
            this.Loading += this.MvvmPage_Loading;
        }

        /// <summary>
        /// View model of this page.
        /// </summary>
        public ViewModelBase ViewModel
        {
            get => (ViewModelBase)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="ViewModel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(ViewModelBase), typeof(MvvmPage), new PropertyMetadata(null, ViewModelPropertyChanged));

        private static void ViewModelPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (ViewModelBase)e.OldValue;
            var newValue = (ViewModelBase)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (MvvmPage)dp;
            sender.OnViewModelChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when <see cref="ViewModel"/> changed.
        /// </summary>
        /// <param name="oldValue">Old view model.</param>
        /// <param name="newValue">New view model.</param>
        protected virtual void OnViewModelChanged(ViewModelBase oldValue, ViewModelBase newValue) { }

        private void MvvmPage_Loading(FrameworkElement sender, object e)
        {
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneShowing;
            InputPane.GetForCurrentView().Hiding += this.InputPane_InputPaneHiding;
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += this.ApplicationView_VisibleBoundsChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += this.TitleBar_LayoutMetricsChanged;
            this.SizeChanged += this.MvvmPage_SizeChanged;
        }

        private void MvvmPage_Unloaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneShowing;
            InputPane.GetForCurrentView().Hiding -= this.InputPane_InputPaneHiding;
            ApplicationView.GetForCurrentView().VisibleBoundsChanged -= this.ApplicationView_VisibleBoundsChanged;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= this.TitleBar_LayoutMetricsChanged;
            this.SizeChanged -= this.MvvmPage_SizeChanged;
        }

        private void InputPane_InputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = true;
            caculateVisibleBoundsThickness(new Size(this.ActualWidth, this.ActualHeight));
        }

        private void InputPane_InputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            caculateVisibleBoundsThickness(new Size(this.ActualWidth, this.ActualHeight));
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            caculateVisibleBoundsThickness(new Size(this.ActualWidth, this.ActualHeight));
        }

        private void ApplicationView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            caculateVisibleBoundsThickness(new Size(this.ActualWidth, this.ActualHeight));
        }

        private void MvvmPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            caculateVisibleBoundsThickness(e.NewSize);
        }

        private void caculateVisibleBoundsThickness(Size size)
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

            var transedView = this.TransformToVisual(null).Inverse.TransformBounds(usedView);

            VisibleBounds = new Thickness(bound(transedView.Left), bound(transedView.Top), bound(size.Width - transedView.Right), bound(size.Height - transedView.Bottom));
        }

        private Thickness visibleBounds = new Thickness();
        /// <summary>
        /// Visible bounds of the page, easiest way to handle this value is binding it to <see cref="Control.Padding"/> of this page.
        /// </summary>
        public Thickness VisibleBounds
        {
            get => this.visibleBounds;
            private set
            {
                if (this.visibleBounds == value)
                    return;
                this.visibleBounds = value;
                SetValue(VisibleBoundsProperty, value);
            }
        }

        /// <summary>
        /// Indentify <see cref="VisibleBounds"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VisibleBoundsProperty =
            DependencyProperty.Register(nameof(VisibleBounds), typeof(Thickness), typeof(MvvmPage), new PropertyMetadata(new Thickness(), VisibleBoundsPropertyChanged));

        private static void VisibleBoundsPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (Thickness)e.OldValue;
            var newValue = (Thickness)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (MvvmPage)dp;
            if (sender.visibleBounds != newValue)
                throw new InvalidOperationException("This property is read only");
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Content == null)
                return new Size();
            var pad = this.Padding;
            this.Content.Measure(new Size(bound(availableSize.Width - pad.Left - pad.Right), bound(availableSize.Height - pad.Top - pad.Bottom)));
            var ns = this.Content.DesiredSize;
            return new Size(ns.Width + pad.Left + pad.Right, ns.Height + pad.Top + pad.Bottom);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Content == null)
                return new Size();
            var pad = this.Padding;
            this.Content.Arrange(new Rect(new Point(pad.Left, pad.Top), new Size(bound(finalSize.Width - pad.Left - pad.Right), bound(finalSize.Height - pad.Top - pad.Bottom))));
            return finalSize;
        }

        private static double bound(double v) => v < 0 ? 0 : v;
    }
}
