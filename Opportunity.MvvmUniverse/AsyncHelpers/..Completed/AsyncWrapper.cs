using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public static class AsyncWrapper
    {
        public static IAsyncAction CreateCompleted()
        {
            return new CompletedAsyncAction(AsyncStatus.Completed, null);
        }

        public static IAsyncActionWithProgress<TProgress> CreateCompletedWithProgress<TProgress>()
        {
            return new CompletedAsyncAction<TProgress>(AsyncStatus.Completed, null);
        }

        public static IAsyncOperation<TResult> CreateCompleted<TResult>(TResult result)
        {
            return new CompletedAsyncOperation<TResult>(AsyncStatus.Completed, result, null);
        }

        public static IAsyncOperationWithProgress<TResult, TProgress> CreateCompletedWithProgress<TResult, TProgress>(TResult result)
        {
            return new CompletedAsyncOperation<TResult, TProgress>(AsyncStatus.Completed, result, null);
        }

        public static IAsyncAction CreateError(Exception error)
        {
            return new CompletedAsyncAction(AsyncStatus.Error, error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncActionWithProgress<TProgress> CreateErrorWithProgress<TProgress>(Exception error)
        {
            return new CompletedAsyncAction<TProgress>(AsyncStatus.Error, error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncOperation<TResult> CreateError<TResult>(Exception error)
        {
            return new CompletedAsyncOperation<TResult>(AsyncStatus.Error, default(TResult), error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncOperationWithProgress<TResult, TProgress> CreateErrorWithProgress<TResult, TProgress>(Exception error)
        {
            return new CompletedAsyncOperation<TResult, TProgress>(AsyncStatus.Error, default(TResult), error ?? throw new ArgumentNullException(nameof(error)));
        }

        public static IAsyncAction CreateCanceled()
        {
            return new CompletedAsyncAction(AsyncStatus.Canceled, new OperationCanceledException());
        }

        public static IAsyncActionWithProgress<TProgress> CreateCanceledWithProgress<TProgress>()
        {
            return new CompletedAsyncAction<TProgress>(AsyncStatus.Canceled, new OperationCanceledException());
        }

        public static IAsyncOperation<TResult> CreateCanceled<TResult>()
        {
            return new CompletedAsyncOperation<TResult>(AsyncStatus.Canceled, default(TResult), new OperationCanceledException());
        }

        public static IAsyncOperationWithProgress<TResult, TProgress> CreateCanceledWithProgress<TResult, TProgress>()
        {
            return new CompletedAsyncOperation<TResult, TProgress>(AsyncStatus.Canceled, default(TResult), new OperationCanceledException());
        }
    }
}
