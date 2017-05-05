using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncWrappers
{
    public sealed partial class AsyncWrapper : IAsyncAction
    {
        public static AsyncWrapper CreateCompleted()
        {
            return new AsyncWrapper(AsyncStatus.Completed, null);
        }

        public static AsyncWrapper<TResult> CreateCompleted<TResult>()
        {
            return new AsyncWrapper<TResult>(AsyncStatus.Completed, default(TResult), null);
        }

        public static AsyncWrapper<TResult> CreateCompleted<TResult>(TResult result)
        {
            return new AsyncWrapper<TResult>(AsyncStatus.Completed, result, null);
        }

        public static AsyncWrapper CreateError(Exception error)
        {
            return new AsyncWrapper(AsyncStatus.Error, error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static AsyncWrapper<TResult> CreateError<TResult>(Exception error)
        {
            return new AsyncWrapper<TResult>(AsyncStatus.Error, default(TResult), error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static AsyncWrapper CreateCanceled()
        {
            return new AsyncWrapper(AsyncStatus.Canceled, new OperationCanceledException());
        }

        public static AsyncWrapper<TResult> CreateCanceled<TResult>()
        {
            return new AsyncWrapper<TResult>(AsyncStatus.Canceled, default(TResult), new OperationCanceledException());
        }
    }
}
