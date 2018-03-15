using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Services.Navigation
{
    /// <summary>
    /// Handler for <see cref="Navigator"/>.
    /// </summary>
    public interface INavigationHandler : IServiceHandler<Navigator>
    {
        /// <summary>
        /// Indicates whether this <see cref="INavigationHandler"/> can handle <see cref="Navigator.GoBackAsync()"/>.
        /// </summary>
        bool CanGoBack { get; }
        /// <summary>
        /// Handles <see cref="Navigator.GoBackAsync()"/>.
        /// </summary>
        /// <returns>Whether the call is handled or not.</returns>
        /// <remarks>This method will be called regardless of <see cref="CanGoBack"/>.</remarks>
        IAsyncOperation<bool> GoBackAsync();
        /// <summary>
        /// Indicates whether this <see cref="INavigationHandler"/> can handle <see cref="Navigator.GoForwardAsync()"/>.
        /// </summary>
        bool CanGoForward { get; }
        /// <summary>
        /// Handles <see cref="Navigator.GoForwardAsync()"/>.
        /// </summary>
        /// <returns>Whether the call is handled or not.</returns>
        /// <remarks>This method will be called regardless of <see cref="CanGoForward"/>.</remarks>
        IAsyncOperation<bool> GoForwardAsync();
        /// <summary>
        /// Handles <see cref="Navigator.NavigateAsync(Type, object)"/>.
        /// </summary>
        /// <returns>Whether the call is handled or not.</returns>
        IAsyncOperation<bool> NavigateAsync(Type sourcePageType, object parameter);
    }
}
