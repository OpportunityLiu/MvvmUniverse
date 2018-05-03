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
                catch
                {
                    return null;
                }
            }
        }

        public static void ThrowUnhandledError(Exception error)
        {
            if (error is null)
                return;
            var e = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error);
            var d = Default;
            if (d is null)
                e.Throw();
            else
                d.Begin(() => e.Throw());
        }
    }
}
