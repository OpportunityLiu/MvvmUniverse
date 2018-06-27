using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class MvvmContentDialog : ContentDialog, IView
    {
        /// <summary>
        /// Create new instance of <see cref="MvvmContentDialog"/>.
        /// </summary>
        public MvvmContentDialog()
        {
            this.Opened += this.MvvmContentDialog_Opened;
            this.Closed += this.MvvmContentDialog_Closed;
        }

        /// <summary>
        /// The opening status of the dialog.
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// View model of this dialog.
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

        private static async void ViewModelPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (ViewModelBase)e.OldValue;
            var newValue = (ViewModelBase)e.NewValue;
            var sender = (MvvmContentDialog)dp;
            if (oldValue == newValue)
            {
                if (newValue != null)
                    newValue.view = sender;
                return;
            }
            if (oldValue != null)
                Interlocked.CompareExchange(ref oldValue.view, null, sender);
            if (newValue != null)
                newValue.view = sender;
            var rt = 0;
            while (!sender.IsOpened && rt < 2)
            {
                await sender.Dispatcher.YieldIdle();
                rt++;
            }
            sender.OnViewModelChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when <see cref="ViewModel"/> changed.
        /// </summary>
        /// <param name="oldValue">Old view model.</param>
        /// <param name="newValue">New view model.</param>
        protected virtual void OnViewModelChanged(ViewModelBase oldValue, ViewModelBase newValue) { }

        private Border BackgroundElement;
        private Grid DialogSpace;

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
            this.BackgroundElement = GetTemplateChild(nameof(this.BackgroundElement)) as Border;
            if (this.BackgroundElement != null && IsOpened)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
            this.DialogSpace = GetTemplateChild(nameof(this.DialogSpace)) as Grid;
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void MvvmContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged += this.BackgroundElement_SizeChanged;
            IsOpened = true;
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneChanging;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged += this.MvvmContentDialog_VisibleBoundsChanged;
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds);
        }

        private void MvvmContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (this.BackgroundElement != null)
                this.BackgroundElement.SizeChanged -= this.BackgroundElement_SizeChanged;
            IsOpened = false;
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneChanging;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged -= this.MvvmContentDialog_VisibleBoundsChanged;
            this.ClearValue(WidthProperty);
            this.ClearValue(HeightProperty);
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
