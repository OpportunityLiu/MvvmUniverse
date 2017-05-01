using System;

namespace Opportunity.MvvmUniverse.Views
{
    public interface INavigationHandler
    {
        bool CanGoBack();
        void GoBack();
        Navigator Parent { get; set; }
    }

    public static class INavigationHandlerExtension
    {
        public static void RaiseCanGoBackChanged(this INavigationHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            handler.Parent?.UpdateAppViewBackButtonVisibility();
        }
    }
}
