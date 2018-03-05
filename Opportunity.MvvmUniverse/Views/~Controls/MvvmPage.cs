using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Opportunity.MvvmUniverse.Views
{
    public class MvvmPage : Page
    {
        public MvvmPage() { }

        protected override Size MeasureOverride(Size availableSize)
        {
            var p = this.TransformToVisual(Window.Current.Content);
            return base.MeasureOverride(availableSize);
        }
    }
}
