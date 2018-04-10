using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Opportunity.MvvmUniverse
{
    internal static class DispatcherHelper
    {
        public static CoreDispatcher Default
        {
            get
            {
                try
                {
                    return CoreApplication.MainView.Dispatcher;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
