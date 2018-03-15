using System;
using Windows.Foundation;

namespace Opportunity.MvvmUniverse.Services.Notification
{
    /// <summary>
    /// Handler for <see cref="Notificator"/>.
    /// </summary>
    public interface INotificationHandler : IServiceHandler<Notificator>
    {
        /// <summary>
        /// Handles notification.
        /// </summary>
        /// <param name="data">Data of notificaiton.</param>
        /// <returns>Whether notification is handled by this <see cref="INotificationHandler"/>.</returns>
        IAsyncOperation<bool> NotifyAsync(object data);
    }

    /// <summary>
    /// Extension methods for <see cref="INotificationHandler"/>.
    /// </summary>
    public static class INotificationHandlerExtension
    {
        /// <summary>
        /// Get <see cref="Notificator"/> associated with the <paramref name="handler"/>.
        /// </summary>
        /// <param name="handler"><see cref="INotificationHandler"/> to find associated <see cref="Notificator"/>.</param>
        /// <returns><see cref="Notificator"/> associated with the <paramref name="handler"/>, or <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
        public static Notificator GetNotificator(this INotificationHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            ServiceHandlerCollection<Notificator, INotificationHandler>.HandlerDic.TryGetValue(handler, out var notificator);
            return notificator;
        }
    }
}
