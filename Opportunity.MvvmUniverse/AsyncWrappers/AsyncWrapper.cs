using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncWrappers
{
    public static class AsyncWrapper
    {
        public static IAsyncAction CreateCompleted()
        {
            return new AsyncActionWrapper(AsyncStatus.Completed, null);
        }

        public static IAsyncActionWithProgress<TProgress> CreateCompletedWithProgress<TProgress>()
        {
            return new AsyncActionWrapper<TProgress>(AsyncStatus.Completed, null);
        }

        public static IAsyncOperation<TResult> CreateCompleted<TResult>(TResult result)
        {
            return new AsyncOperationWrapper<TResult>(AsyncStatus.Completed, result, null);
        }

        public static IAsyncOperationWithProgress<TResult, TProgress> CreateCompletedWithProgress<TResult, TProgress>(TResult result)
        {
            return new AsyncOperationWrapper<TResult, TProgress>(AsyncStatus.Completed, result, null);
        }

        public static IAsyncAction CreateError(Exception error)
        {
            return new AsyncActionWrapper(AsyncStatus.Error, error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncActionWithProgress<TProgress> CreateErrorWithProgress<TProgress>(Exception error)
        {
            return new AsyncActionWrapper<TProgress>(AsyncStatus.Error, error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncOperation<TResult> CreateError<TResult>(Exception error)
        {
            return new AsyncOperationWrapper<TResult>(AsyncStatus.Error, default(TResult), error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncOperationWithProgress<TResult, TProgress> CreateErrorWithProgress<TResult, TProgress>(Exception error)
        {
            return new AsyncOperationWrapper<TResult, TProgress>(AsyncStatus.Error, default(TResult), error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncAction CreateCanceled()
        {
            return new AsyncActionWrapper(AsyncStatus.Canceled, new OperationCanceledException());
        }

        public static IAsyncActionWithProgress<TProgress> CreateCanceledWithProgress<TProgress>()
        {
            return new AsyncActionWrapper<TProgress>(AsyncStatus.Canceled, new OperationCanceledException());
        }

        public static IAsyncOperation<TResult> CreateCanceled<TResult>()
        {
            return new AsyncOperationWrapper<TResult>(AsyncStatus.Canceled, default(TResult), new OperationCanceledException());
        }

        public static IAsyncOperationWithProgress<TResult, TProgress> CreateCanceledWithProgress<TResult, TProgress>()
        {
            return new AsyncOperationWrapper<TResult, TProgress>(AsyncStatus.Canceled, default(TResult), new OperationCanceledException());
        }
    }
}
