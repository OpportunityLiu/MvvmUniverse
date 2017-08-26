using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public static class CastWrapper
    {
        public static IAsyncOperation<TTo> Cast<TFrom, TTo>(this IAsyncOperation<TFrom> operation)
        {
            return new CastAcyncOperation<TFrom, TTo>(operation);
        }

        public static IAsyncOperationWithProgress<TTo, TProgress> Cast<TFrom, TTo, TProgress>(this IAsyncOperationWithProgress<TFrom, TProgress> operation)
        {
            return new CastAcyncOperation<TFrom, TTo, TProgress>(operation);
        }

        public static IAsyncAction AsAsyncAction<T>(this IAsyncOperation<T> operation)
        {
            return new CastAsyncAction<T>(operation);
        }

        public static IAsyncActionWithProgress<TProgress> AsAsyncAction<T, TProgress>(this IAsyncOperationWithProgress<T, TProgress> operation)
        {
            return new CastAsyncAction<T, TProgress>(operation);
        }
    }
}
