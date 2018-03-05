using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Views
{
    public interface INavigationHandler
    {
        bool CanGoBack { get; }
        IAsyncOperation<bool> GoBackAsync();
        bool CanGoForward { get; }
        IAsyncOperation<bool> GoForwardAsync();
        IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter);
        void OnAdd(Navigator navigator);
        void OnRemove();
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
