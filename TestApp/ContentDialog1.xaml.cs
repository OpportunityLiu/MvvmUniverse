using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace TestApp
{
    public sealed partial class ContentDialog1 : MvvmContentDialog
    {
        public ContentDialog1()
        {
            this.InitializeComponent();
            this.tbContent.Text = @"* Show user rating  
  ![image](https://user-images.githubusercontent.com/13471233/36630230-f487930e-199d-11e8-8336-5ab6515c419b.png)  
* Scan QR code in gallery images  
  ![image](https://user-images.githubusercontent.com/13471233/36630227-eceeb6f4-199d-11e8-8398-f34f4d36132f.png)
* Bug fix & other improvements";
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
