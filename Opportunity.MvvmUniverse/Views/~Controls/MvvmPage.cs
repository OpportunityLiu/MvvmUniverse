using Opportunity.Helpers.Universal.AsyncHelpers;
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
using Windows.UI.ViewManagement.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Page with accociated view model and supports <see cref="VisibleBounds"/>.
    /// </summary>
    public class MvvmPage : Page, IView
    {
        /// <summary>
        /// Create new instance of <see cref="MvvmPage"/>.
        /// </summary>
        public MvvmPage()
        {
            this.Unloaded += this.MvvmPage_Unloaded;
            this.Loading += this.MvvmPage_Loading;
            this.Loaded += this.MvvmPage_Loaded;
        }

        /// <summary>
        /// The title of current page.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(MvvmPage), new PropertyMetadata(null, TitlePropertyChanged));

        private static void TitlePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (string)e.OldValue;
            var newValue = (string)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (MvvmPage)dp;
            if (!sender.IsLoaded)
                return;
            ApplicationView.GetForCurrentView().Title = newValue ?? "";
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

        private static async void ViewModelPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (ViewModelBase)e.OldValue;
            var newValue = (ViewModelBase)e.NewValue;
            var sender = (MvvmPage)dp;
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
            while (!sender.IsLoaded && rt < 2)
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

        /// <summary>
        /// The loading status of current page.
        /// </summary>
        public bool IsLoaded { get; private set; }

        private void MvvmPage_Loading(FrameworkElement sender, object e)
        {
            InputPane.GetForCurrentView().Showing += this.InputPane_InputPaneShowing;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged += this.MvvmPage_VisibleBoundsChanged;
            this.SizeChanged += this.MvvmPage_SizeChanged;
            if (Title is string t)
                ApplicationView.GetForCurrentView().Title = t;
        }

        private void MvvmPage_Loaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = true;
        }

        private void MvvmPage_Unloaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = false;
            var v = ApplicationView.GetForCurrentView();
            if (v.Title == Title)
            {
                var t = "";
                foreach (var item in Window.Current.Content?.Descendants<MvvmPage>().EmptyIfNull())
                {
                    if (!item.IsLoaded)
                        continue;
                    var tt = item.Title;
                    if (tt is null)
                        continue;
                    t = tt;
                }
                v.Title = t;
            }
            InputPane.GetForCurrentView().Showing -= this.InputPane_InputPaneShowing;
            VisibleBoundsHelper.GetForCurrentView().VisibleBoundsChanged -= this.MvvmPage_VisibleBoundsChanged;
            this.SizeChanged -= this.MvvmPage_SizeChanged;
        }

        private void MvvmPage_VisibleBoundsChanged(object sender, Rect e)
        {
            caculateVisibleBoundsThickness(e, new Size(this.ActualWidth, this.ActualHeight));
        }

        private void InputPane_InputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = true;
        }

        private void MvvmPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            caculateVisibleBoundsThickness(VisibleBoundsHelper.GetForCurrentView().VisibleBounds, e.NewSize);
        }

        private void caculateVisibleBoundsThickness(Rect vb, Size size)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            var transedView = this.TransformToVisual(null).Inverse.TransformBounds(vb);
            VisibleBounds = new Thickness(transedView.Left.BoundToZero(), transedView.Top.BoundToZero(), (size.Width - transedView.Right).BoundToZero(), (size.Height - transedView.Bottom).BoundToZero());
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
                if (MathHelper.Diff(this.visibleBounds, value) < 4)
                    return;
                Debug.WriteLine(value);
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
            this.Content.Measure(new Size(
                (availableSize.Width - pad.Left - pad.Right).BoundToZero(),
                (availableSize.Height - pad.Top - pad.Bottom).BoundToZero()));
            var ns = this.Content.DesiredSize;
            return new Size(ns.Width + pad.Left + pad.Right, ns.Height + pad.Top + pad.Bottom);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Content == null)
                return new Size();
            var pad = this.Padding;
            this.Content.Arrange(new Rect(new Point(pad.Left, pad.Top), new Size(
                (finalSize.Width - pad.Left - pad.Right).BoundToZero(),
                (finalSize.Height - pad.Top - pad.Bottom).BoundToZero())));
            return finalSize;
        }
    }
}
