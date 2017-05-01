using System;

namespace Opportunity.MvvmUniverse.Views
{
    public interface INavigationHandler
    {
        bool CanGoBack();
        void GoBack();
        event EventHandler CanGoBackChanged;
    }
}
