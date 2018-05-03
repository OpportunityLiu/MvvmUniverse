using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;

namespace Opportunity.MvvmUniverse.Commands
{
    internal static class CommandHelper
    {
        public static Exception GetError(IAsyncAction execution)
        {
            switch (execution.Status)
            {
            case AsyncStatus.Canceled:
                return new OperationCanceledException();
            case AsyncStatus.Error:
                return execution.ErrorCode;
            }

            return null;
        }

        public static async void ThrowIfUnhandled(ExecutedEventArgs args)
        {
            if (args.Exception is null)
                return;
            // wait for handling
            if (DispatcherHelper.Default is CoreDispatcher d)
                await d.YieldIdle();
            else
                await Task.Delay(50).ConfigureAwait(false);
            if (!args.Handled)
                DispatcherHelper.ThrowUnhandledError(args.Exception);
        }

        public static void ResetCurrent(ref IAsyncAction current, IAsyncAction execution)
        {
            if (execution != Interlocked.CompareExchange(ref current, null, execution))
                throw new InvalidOperationException("execution != Current");
        }

        public static void SetCurrent(ref IAsyncAction current, IAsyncAction execution)
        {
            if (Interlocked.CompareExchange(ref current, execution, null) is null)
                return;
            throw new InvalidOperationException("Current is not null.");
        }

        public static void AssertCurrentEquals(IAsyncAction current, IAsyncAction execution)
        {
            if (execution != current)
                throw new InvalidOperationException("execution != Current");
        }

        public static void AssertCurrentIsNull(IAsyncAction current)
        {
            if (current is null)
                return;
            throw new InvalidOperationException("Current is not null.");
        }
    }
}
