using System;

namespace Opportunity.MvvmUniverse.Views
{
    public interface INavigationHandler
    {
        bool CanGoBack();
        void GoBack();
    }

    public static class INavigationHandlerExtension
    {
        public static void RaiseCanGoBackChanged(this INavigationHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            if (!NavigationHandlerCollection.NavigationHandlerDic.TryGetValue(handler, out var navigator))
                return;
            navigator.UpdateAppViewBackButtonVisibility();
        }

        public static Navigator GetNavigator(this INavigationHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            NavigationHandlerCollection.NavigationHandlerDic.TryGetValue(handler, out var navigator);
            return navigator;
        }
    }
}
