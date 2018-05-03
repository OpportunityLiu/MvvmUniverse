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

        internal static void ThrowUnhandledError(Exception error)
        {
            if (error is null)
                return;
            var d = Default;
            if (d is null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
            else if (d.HasThreadAccess)
                throwCore(error);
            else
                d.Begin(() => throwCore(error));
        }

        private static void throwCore(Exception error)
        {
            if (error is AggregateException ae)
                throw new AggregateException(ae.InnerExceptions);
            else
                throw new AggregateException(error);
        }
    }
}
