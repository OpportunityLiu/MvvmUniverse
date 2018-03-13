using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Views
{
    /// <summary>
    /// Handler for <see cref="Navigator"/>.
    /// </summary>
    public interface INavigationHandler
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
        /// <summary>
        /// Will be called when adding to <see cref="Navigator.Handlers"/>.
        /// </summary>
        /// <param name="navigator">The <see cref="Navigator"/> of which is adding to.</param>
        void OnAdd(Navigator navigator);
        /// <summary>
        /// Will be called when removing from <see cref="Navigator.Handlers"/>.
        /// </summary>
        void OnRemove();
    }

    /// <summary>
    /// Extension methods for <see cref="INavigationHandler"/>.
    /// </summary>
    public static class INavigationHandlerExtension
    {
        /// <summary>
        /// Get <see cref="Navigator"/> associated with the <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler"><see cref="INavigationHandler"/> to find <see cref="Navigator"/>.</param>
        /// <returns><see cref="Navigator"/> associated with the <paramref name="handler"/>, or <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        public static Navigator GetNavigator(this INavigationHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            NavigationHandlerCollection.NavigationHandlerDic.TryGetValue(handler, out var navigator);
            return navigator;
        }
    }
}
