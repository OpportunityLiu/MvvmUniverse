using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Views
{
    public interface INavigationHandler
    {
        bool CanGoBack();
        IAsyncOperation<bool> GoBackAsync();
        bool CanGoForward();
        IAsyncOperation<bool> GoForwardAsync();
        IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter);
    }

    public static class INavigationHandlerExtension
    {
        public static Navigator GetNavigator(this INavigationHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            NavigationHandlerCollection.NavigationHandlerDic.TryGetValue(handler, out var navigator);
            return navigator;
        }
    }
}
