using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.AsyncHelpers
{
    public static class MulticastExtension
    {
        public static MulticastAsyncAction AsMulticast(this IAsyncAction action)
            => new MulticastAsyncAction(action);
        public static MulticastAsyncAction<TProgress> AsMulticast<TProgress>(this IAsyncActionWithProgress<TProgress> action)
            => new MulticastAsyncAction<TProgress>(action);
        public static MulticastAsyncOperation<T> AsMulticast<T>(this IAsyncOperation<T> operation)
            => new MulticastAsyncOperation<T>(operation);
        public static MulticastAsyncOperation<T, TProgress> AsMulticast<T, TProgress>(this IAsyncOperationWithProgress<T, TProgress> operation)
            => new MulticastAsyncOperation<T, TProgress>(operation);
    }
}
