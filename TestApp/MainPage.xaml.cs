﻿using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Commands.Predefined;
using Opportunity.MvvmUniverse.Services.Navigation;
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
using Windows.UI.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.BulkAccess;
using Windows.Storage.Pickers;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.Concurrent;
using Opportunity.MvvmUniverse.Commands.ReentrancyHandlers;
using Opportunity.MvvmUniverse.Collections;
using Microsoft.Toolkit.Services.OneDrive;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace TestApp
{
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public sealed class TestME : MarkupExtension
    {
        public TestME()
        {
        }
        public TestME(string value) { this.Value = value; }

        public string Value { get; set; }

        protected override object ProvideValue()
        {
            return Value;
        }
    }

    public class VM : ViewModelBase { public string Id => GetHashCode().ToString(); }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : MvvmPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            // this.xp.RegisterPropertyChangedCallback(VisibleBoundsProperty, VBC);
            var c = AsyncActionCommand<int>.Create(async (s, i, t) =>
            {
                Debug.WriteLine($"Enter {i}");
                try
                {
                    await Task.Delay(1000, t);
                    Debug.WriteLine($"Exit {i}");
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Cancel {i}");
                    throw;
                }
            });
            c.ReentrancyHandler = ReentrancyHandler.Queued<int>();
            c.Executed += (s, e) => e.Handled = true;
            this.btnTest.Command = c;
            this.btnTest.CommandParameter = 0;
        }

        // private void VBC(DependencyObject sender, DependencyProperty dp) => Debug.WriteLine(this.xp.VisibleBounds);

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

        private void MvvmPage_Loaded(object sender, RoutedEventArgs e)
        {
            View.CurrentChanged += this.View_CurrentChanged;
            View.CurrentChanging += this.View_CurrentChanging;
            Bindings.Update();
            var x = LoadItemsResult.Create(12, new[] { 1, 2, 3 });
            var d = new ArraySegment<int>(new[] { 1, 2, 3 }, 1, 2);
        }

        private void View_CurrentChanged(object sender, object e)
        {
            Bindings.Update();
        }

        public ICollectionView View = DataList.View;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            navigator.GoBackAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            navigator.GoForwardAsync();
        }


        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OneDriveService.ServicePlatformInitializer = new Microsoft.Toolkit.Uwp.Services.OneDrive.Platform.OneDriveServicePlatformInitializer();
            OneDriveService.Instance.Initialize("daf2a654-5b98-4954-bd03-d09a375628ed", new[] { Microsoft.Toolkit.Services.Services.MicrosoftGraph.MicrosoftGraphScope.FilesReadAll });
            await OneDriveService.Instance.LoginAsync();
            var rf = await OneDriveService.Instance.RootFolderAsync();
            var mf = await rf.GetFolderAsync("Music");

            var files = await getFile(mf);

            var musicPropertis = files.Select(f => f.OneDriveItem.Audio);

            async Task<List<OneDriveStorageFile>> getFile(OneDriveStorageFolder folder)
            {
                var items = await folder.GetItemsAsync(1000);
                var list = new List<OneDriveStorageFile>(items.Count);
                var getFolder = new List<Task<List<OneDriveStorageFile>>>();
                foreach (var item in items)
                {
                    if (item.IsFile())
                        list.Add((OneDriveStorageFile)item);
                    else if (item.IsFolder())
                        getFolder.Add(getFile((OneDriveStorageFolder)item));
                    else
                        continue;// IsOneNote
                }
                foreach (var item in getFolder)
                {
                    list.AddRange(await item);
                }
                return list;
            }
        }

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var f = await StorageFolder.GetFolderFromPathAsync(Path.Combine(@"C:\Users\lzy\OneDrive\", Uri.UnescapeDataString(@"Music\Ace%20Combat%20X%C2%B2")));
            this.btnTest.CommandParameter = (int)this.btnTest.CommandParameter + 1;
            Debug.WriteLine($"Inc: { this.btnTest.CommandParameter }");
            //var appview = CoreApplication.CreateNewView();
            //var id = await appview.Dispatcher.RunAsync(async () =>
            //{
            //    var rootFrame = Window.Current.Content as Frame;

            //    // 不要在窗口已包含内容时重复应用程序初始化，
            //    // 只需确保窗口处于活动状态
            //    if (rootFrame == null)
            //    {
            //        // 创建要充当导航上下文的框架，并导航到第一页
            //        rootFrame = new Frame();
            //        rootFrame.Margin = new Thickness(10);
            //        rootFrame.Padding = new Thickness(10);
            //        ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);

            //        Navigator.GetOrCreateForCurrentView().Handlers.Add(rootFrame.AsNavigationHandler());

            //        // 将框架放在当前窗口中
            //        Window.Current.Content = rootFrame;
            //    }
            //    if (rootFrame.Content == null)
            //    {
            //        // 当导航堆栈尚未还原时，导航到第一页，
            //        // 并通过将所需信息作为导航参数传入来配置
            //        // 参数
            //        await Navigator.GetForCurrentView().NavigateAsync(typeof(MainPage));
            //    }
            //    // 确保当前窗口处于活动状态
            //    Window.Current.Activate();
            //    return ApplicationView.GetForCurrentView().Id;
            //});
            //await ApplicationViewSwitcher.TryShowAsStandaloneAsync(id);
            //await ApplicationViewSwitcher.SwitchAsync(id);
        }
    }
}
