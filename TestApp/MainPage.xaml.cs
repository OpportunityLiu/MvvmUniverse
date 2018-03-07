using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace TestApp
{
    public class VM : ViewModelBase { public string Id => GetHashCode().ToString(); }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : MvvmPage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public new VM ViewModel { get => (VM)base.ViewModel; set => base.ViewModel = value; }

        protected override void OnViewModelChanged(ViewModelBase oldValue, ViewModelBase newValue)
        {
            var o = (VM)oldValue;
            var n = (VM)newValue;
        }

        private Navigator navigator = Navigator.GetForCurrentView();

        private void View_CurrentChanging(object sender, CurrentChangingEventArgs e)
        {
            if (e.IsCancelable)
            {
                //e.Cancel = true;
                Debug.WriteLine("IsCancelable");
            }
            else
            {
                Debug.WriteLine("IsNotCancelable");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (Source != null)
            {
                Source.View.CurrentChanging -= this.View_CurrentChanging;
                Source.View.CurrentChanged -= this.View_CurrentChanged;
            }
            Source = (CollectionViewSource)e.Parameter ?? new CollectionViewSource { Source = Data };
            Source.View.CurrentChanged += this.View_CurrentChanged;
            Source.View.CurrentChanging += this.View_CurrentChanging;
            Bindings.Update();
        }

        private void View_CurrentChanged(object sender, object e)
        {
            Bindings.Update();
        }

        public DataList Data = new DataList();

        public CollectionViewSource Source;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            navigator.GoBackAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            navigator.GoForwardAsync();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.ViewModel = new VM();
            ApplicationView.GetForCurrentView().IsScreenCaptureEnabled = false;
            Grid.SetColumn(xp, 0);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            Grid.SetColumn(xp, 1);
        }
    }
}
