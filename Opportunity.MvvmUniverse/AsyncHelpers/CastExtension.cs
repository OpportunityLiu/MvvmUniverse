using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public static class CastExtension
    {
        public static IAsyncOperation<TTo> Cast<TFrom, TTo>(this IAsyncOperation<TFrom> operation)
        {
            return new CastAcyncOperation<TFrom, TTo>(operation);
        }

        public static IAsyncOperationWithProgress<TTo, TProgress> Cast<TFrom, TTo, TProgress>(this IAsyncOperationWithProgress<TFrom, TProgress> operation)
        {
            return new CastAcyncOperation<TFrom, TTo, TProgress>(operation);
        }
    }
}
