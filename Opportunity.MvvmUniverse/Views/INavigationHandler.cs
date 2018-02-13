using System;

namespace Opportunity.MvvmUniverse.Views
{
    public interface INavigationHandler
    {
        bool CanGoBack();
        void GoBack();
        bool CanGoForward();
        void GoForward();
        bool Navigate(Type sourcePageType, object parameter);
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
